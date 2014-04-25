using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JustPressPlay.Models.Repositories;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.ComponentModel.DataAnnotations;

using System.Net;
using System.Net.Mail;
using SendGridMail;
using SendGridMail.Transport;
using ZXing;
using ZXing.Common;

namespace JustPressPlay.Utilities
{
    public class JPPNewsFeed
    {
        public String Title { get; set; }
        public String Icon { get; set; }
        public String Text { get; set; }
        public String Controller { get; set; }
        public String Action { get; set; }
        public int ID { get; set; }

        public List<JPPNewsFeed> Populate()
        {
            List<JPPNewsFeed> fullNewsFeedList = new List<JPPNewsFeed>();

            UnitOfWork work = new UnitOfWork();

            fullNewsFeedList.AddRange(work.AchievementRepository.GetAchievementsForFeed());
            fullNewsFeedList.AddRange(work.QuestRepository.GetQuestsForFeed());
            fullNewsFeedList.AddRange(work.SystemRepository.GetNewsForFeed());

            return fullNewsFeedList;
        }
    }

    public class JPPImage
    {
        /// <summary>
        /// Holds info about the image for saving
        /// </summary>
        public class ImageSaveInfo
        {
            public enum ImageType { Player, Achievement, SystemQuest, CommunityQuest };

            public int Create { get; set; }
            public int Explore { get; set; }
            public int Learn { get; set; }
            public int Socialize { get; set; }
            public ImageType Type { get; set; }

            public ImageSaveInfo(int create, int explore, int learn, int socialize, ImageType type)
            {
                Create = create;
                Explore = explore;
                Learn = learn;
                Socialize = socialize;
                Type = type;
            }
        }

        /// <summary>
        /// Used for determining which side of an image is larger
        /// </summary>
        private enum LargerSide { Width, Height, Same };

        /// <summary>
        /// Gets the achievement/quest icon file names in the system, WITHOUT extensions
        /// </summary>
        /// <returns>A list of file names (or an empty list if none are found)</returns>
        public static List<String> GetIconFileNames()
        {
            // Get the png files
            DirectoryInfo di = new DirectoryInfo(HttpContext.Current.Server.MapPath(JPPConstants.Images.IconPath));
            FileInfo[] files = di.GetFiles("*.png");

            // Add just the names
            List<String> imageList = new List<string>();
            foreach (FileInfo f in files)
            {
                imageList.Add(f.Name.Replace(".png", ""));
            }

            return imageList;
        }

        /// <summary>
        /// Checks to make sure the image uploaded is one of the approved types (.png, .gif, .jpg)
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool FileIsWebFriendlyImage(Stream stream)
        {
            //Try to grab the image from the stream
            try
            {
                //Read an image from the stream...
                var i = Image.FromStream(stream);

                //Move the pointer back to the beginning of the stream
                stream.Seek(0, SeekOrigin.Begin);

                return ImageFormat.Png.Equals(i.RawFormat) || ImageFormat.Gif.Equals(i.RawFormat) || ImageFormat.Jpeg.Equals(i.RawFormat);
            }
            catch
            {
                return false;
            }
        }

		/// <summary>
		/// Saves the three player images
		/// </summary>
		/// <param name="filePath">The file path (no file name)</param>
		/// <param name="fileNameNoExt">The file name without extension</param>
		/// <param name="stream">The image stream</param>
		public static Boolean SavePlayerImages(string filePath, string fileNameNoExt, Stream stream)
		{
			try
			{
				Image image = Image.FromStream(stream);
				ImageSaveInfo info = new ImageSaveInfo(0, 0, 0, 0, ImageSaveInfo.ImageType.Player);
                String savePath = "";
                if (HttpContext.Current.Server.MapPath(filePath).Contains(".jpg"))
                {
                    savePath = HttpContext.Current.Server.MapPath(filePath).Replace(".jpg", "");
                }
                savePath = HttpContext.Current.Server.MapPath(filePath).Replace(".png", "");
                

                SaveImageAtSquareSize(savePath + "_s.png", image, JPPConstants.Images.SizeSmall, info);
                SaveImageAtSquareSize(savePath + "_m.png", image, JPPConstants.Images.SizeMedium, info);
                SaveImageAtSquareSize(savePath + ".png", image, JPPConstants.Images.SizeLarge, info);

                image.Dispose();

                return true;
            }
            catch
            {
                // Problem
                return false;
            }
        }


        /// <summary>
        /// Saves the three player qrcodes
        /// </summary>
        /// <param name="filePath">The file path (no file name)</param>
        /// <param name="fileNameNoExt">The file name without extension</param>
        /// <param name="stream">The image stream</param>
        public static Boolean SavePlayerQRCodes(string filePath, string fileNameNoExt, string qrString)
        {

            var qrValue = qrString;
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    Margin = 0
                }
            };

            using (var bitmap = barcodeWriter.Write(qrValue))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);



                try
                {
                    Image image = Image.FromStream(stream);
                    ImageSaveInfo info = new ImageSaveInfo(0, 0, 0, 0, ImageSaveInfo.ImageType.Player);
                    String savePath = "";
                    if (HttpContext.Current.Server.MapPath(filePath).Contains(".jpg"))
                    {
                        savePath = HttpContext.Current.Server.MapPath(filePath).Replace(".jpg", "");
                    }
                    savePath = HttpContext.Current.Server.MapPath(filePath).Replace(".png", "");


                    SaveImageAtSquareSize(savePath + "_s.png", image, JPPConstants.Images.SizeSmall, info);
                    SaveImageAtSquareSize(savePath + "_m.png", image, JPPConstants.Images.SizeMedium, info);
                    SaveImageAtSquareSize(savePath + ".png", image, JPPConstants.Images.SizeLarge, info);

                    image.Dispose();

                    return true;
                }
                catch
                {
                    // Problem
                    return false;
                }
            }
        }

      

        /// <summary>
        /// Saves the three achievement icons
        /// </summary>
        /// <param name="newFileNameAndPath">The file and path for saving the images</param>
        /// <param name="iconNameNoExt">The icon name without extension</param>
        /// <param name="create">Create points for the achievement</param>
        /// <param name="explore">Explore points for the achievement</param>
        /// <param name="learn">Learn points for the achievement</param>
        /// <param name="socialize">Socialize points for the achievement</param>
        public static bool SaveAchievementIcons(string newFileNameAndPath, string iconNameNoExt, int create, int explore, int learn, int socialize)
        {
            String imagePath = "";
            try
            {
                Image image = Image.FromFile(HttpContext.Current.Server.MapPath(JPPConstants.Images.IconPath + iconNameNoExt + ".png"));

                ImageSaveInfo info = new ImageSaveInfo(create, explore, learn, socialize, ImageSaveInfo.ImageType.Achievement);
                String savePath = "";
                if (HttpContext.Current.Server.MapPath(newFileNameAndPath).Contains(".jpg"))
                {
                    savePath = HttpContext.Current.Server.MapPath(newFileNameAndPath).Replace(".jpg", "");
                }
                savePath = HttpContext.Current.Server.MapPath(newFileNameAndPath).Replace(".png", "");
                imagePath = savePath;
                SaveImageAtSquareSize(savePath + "_s.png", image, JPPConstants.Images.SizeSmall, info);
                SaveImageAtSquareSize(savePath + "_m.png", image, JPPConstants.Images.SizeMedium, info);
                SaveImageAtSquareSize(savePath + ".png", image, JPPConstants.Images.SizeLarge, info);

                image.Dispose();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the three quest icons
        /// </summary>
        /// <param name="newFileNameAndPath">The file and path for saving the images</param>
        /// <param name="iconNameNoExt">The icon name without extension</param>
        /// <param name="userGeneratedQuest">Is this a user generated quest?</param>
        public static Boolean SaveQuestIcons(string newFileNameAndPath, string iconNameNoExt, bool userGeneratedQuest)
        {
            try
            {
                Image image = Image.FromFile(HttpContext.Current.Server.MapPath(JPPConstants.Images.IconPath + iconNameNoExt + ".png"));
                ImageSaveInfo info = new ImageSaveInfo(0, 0, 0, 0, userGeneratedQuest ? ImageSaveInfo.ImageType.CommunityQuest : ImageSaveInfo.ImageType.SystemQuest);
                String savePath = "";
                if (HttpContext.Current.Server.MapPath(newFileNameAndPath).Contains(".jpg"))
                {
                    savePath = HttpContext.Current.Server.MapPath(newFileNameAndPath).Replace(".jpg", "");
                }

                savePath = HttpContext.Current.Server.MapPath(newFileNameAndPath).Replace(".png", "");

                SaveImageAtSquareSize(savePath + "_s.png", image, JPPConstants.Images.SizeSmall, info);
                SaveImageAtSquareSize(savePath + "_m.png", image, JPPConstants.Images.SizeMedium, info);
                SaveImageAtSquareSize(savePath + ".png", image, JPPConstants.Images.SizeLarge, info);

                image.Dispose();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves an image (from a stream) at a specific size
        /// </summary>
        /// <param name="filePath">The file name and path for saving</param>
        /// <param name="stream">The image stream</param>
        /// <param name="size">The size for saving</param>
        public static void SaveImageAtSquareSize(string filePath, Image originalImage, int size, ImageSaveInfo info)
        {
            // New empty image and graphics for manipulation
            Bitmap newImage = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(newImage);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Size cases
            LargerSide largerSide = LargerSide.Same;
            if (originalImage.Width > originalImage.Height) largerSide = LargerSide.Width;
            else if (originalImage.Height > originalImage.Width) largerSide = LargerSide.Height;

            int offsetWidth = 0;
            int offsetHeight = 0;
            int scaledWidth = originalImage.Width;
            int scaledHeight = originalImage.Height;
            float scale = 1.0f;

            switch (largerSide)
            {
                // Both are the same, just use width calculation
                case LargerSide.Same:

                // Width is longer, so stretch on height
                case LargerSide.Width: scale = size / (float)originalImage.Height; break;

                // Height is longer, so stretch on width
                case LargerSide.Height: scale = size / (float)originalImage.Width; break;
            }

            // Figure out offsets
            scaledWidth = (int)(originalImage.Width * scale);
            scaledHeight = (int)(originalImage.Height * scale);
            offsetHeight = (size - scaledHeight) / 2;
            offsetWidth = (size - scaledWidth) / 2;

            // Handle achievements and quests (do nothing here for Players)
            int inset = (int)(size * JPPConstants.Images.QuadBorderOffsetPercent);
            int insetSize = size - (inset * 2) - 1; // Adjust slightly for smaller images
            switch (info.Type)
            {
                case ImageSaveInfo.ImageType.Achievement:
                    // Quads
                    Brush create = new SolidBrush(info.Create <= 0 ? JPPConstants.Images.QuadCreateOffColor : JPPConstants.Images.QuadCreateOnColor);
                    Brush explore = new SolidBrush(info.Explore <= 0 ? JPPConstants.Images.QuadExploreOffColor : JPPConstants.Images.QuadExploreOnColor);
                    Brush learn = new SolidBrush(info.Learn <= 0 ? JPPConstants.Images.QuadLearnOffColor : JPPConstants.Images.QuadLearnOnColor);
                    Brush social = new SolidBrush(info.Socialize <= 0 ? JPPConstants.Images.QuadSocializeOffColor : JPPConstants.Images.QuadSocializeOnColor);
                    g.FillPie(create, 0, 0, size, size, 180, 90);
                    g.FillPie(explore, 0, 0, size, size, 0, 90);
                    g.FillPie(learn, 0, 0, size, size, 270, 90);
                    g.FillPie(social, 0, 0, size, size, 90, 90);

                    // Adjust image
                    offsetWidth += inset;
                    offsetHeight += inset;
                    scaledWidth -= (inset * 2);
                    scaledHeight -= (inset * 2);

                    break;

                case ImageSaveInfo.ImageType.SystemQuest:
                case ImageSaveInfo.ImageType.CommunityQuest:
                    // Background
                    Brush brush = new SolidBrush(info.Type == ImageSaveInfo.ImageType.SystemQuest ? JPPConstants.Images.QuestSystemColor : JPPConstants.Images.QuestCommunityColor);
                    g.FillPie(brush, 0, 0, size, size, 0, 360);

                    // Adjust image
                    offsetWidth += inset;
                    offsetHeight += inset;
                    scaledWidth -= (inset * 2);
                    scaledHeight -= (inset * 2);

                    break;
            }

            // Draw the image
            g.DrawImage(originalImage, offsetWidth, offsetHeight, scaledWidth, scaledHeight);

            // Now put the borders on
            if (info.Type != ImageSaveInfo.ImageType.Player)
            {
                // Clip the edges
                Pen clipPen = new Pen(Color.FromArgb(0, 255, 255, 255), size * JPPConstants.Images.QuadBorderWidthPercent * 12);
                int clipInset = (int)(size * -0.1f) - 1; // Adjust for smaller images
                int clipSize = size - (clipInset * 2);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawArc(clipPen, clipInset, clipInset, clipSize, clipSize, 0, 360);
                g.CompositingMode = CompositingMode.SourceOver;

                // Set up widths
                int penWidth = (int)(size * JPPConstants.Images.QuadBorderOffsetPercent);
                int halfWidth = penWidth / 2;

                switch (info.Type)
                {
                    case ImageSaveInfo.ImageType.Achievement:
                        // Quads
                        Pen create = new Pen(info.Create <= 0 ? JPPConstants.Images.QuadCreateOffColor : JPPConstants.Images.QuadCreateOnColor, penWidth);
                        Pen explore = new Pen(info.Explore <= 0 ? JPPConstants.Images.QuadExploreOffColor : JPPConstants.Images.QuadExploreOnColor, penWidth);
                        Pen learn = new Pen(info.Learn <= 0 ? JPPConstants.Images.QuadLearnOffColor : JPPConstants.Images.QuadLearnOnColor, penWidth);
                        Pen social = new Pen(info.Socialize <= 0 ? JPPConstants.Images.QuadSocializeOffColor : JPPConstants.Images.QuadSocializeOnColor, penWidth);
                        g.DrawArc(create, halfWidth, halfWidth, size - penWidth, size - penWidth, 180, 90);
                        g.DrawArc(explore, halfWidth, halfWidth, size - penWidth, size - penWidth, 0, 90);
                        g.DrawArc(learn, halfWidth, halfWidth, size - penWidth, size - penWidth, 270, 90);
                        g.DrawArc(social, halfWidth, halfWidth, size - penWidth, size - penWidth, 90, 90);
                        break;

                    case ImageSaveInfo.ImageType.CommunityQuest:
                    case ImageSaveInfo.ImageType.SystemQuest:
                        Pen pen = new Pen(
                            info.Type == ImageSaveInfo.ImageType.SystemQuest ? JPPConstants.Images.QuestSystemColor : JPPConstants.Images.QuestCommunityColor,
                            penWidth);
                        g.DrawArc(pen, halfWidth, halfWidth, size - penWidth, size - penWidth, 0, 360);
                        break;
                }

                // White Border
                Pen borderPen = new Pen(Color.FromKnownColor(KnownColor.White), size * JPPConstants.Images.QuadBorderWidthPercent);
                g.DrawArc(borderPen, inset, inset, insetSize, insetSize, 0, 360);
            }

            // All done
                newImage.Save(filePath, ImageFormat.Png);
            
            

            // Cleanup
            newImage.Dispose();
        }

        //TODO: Update this so that it actually crops the image instead of just skewing it
        /// <summary>
        /// Resizes and saves the image to the specified location
        /// </summary>
        /// <param name="savePath">The Filepath where the image will be saved</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="stream">The bytes for the image passed in</param>
        /// <param name="maxSideSize">Maximum image width</param>
        /// <param name="makeItSquare">Whether or not to make the image a square</param>
        public static void Save(HttpServerUtilityBase serverUtilityBase, string filePath, Stream stream, int maxSideSize, int minSideSize, bool makeItSquare)
        {
            HttpServerUtilityBase server = serverUtilityBase;

            //Grab the image from the stream
            Image image = Image.FromStream(stream);

            //Establish the height and width based on the max side size
            int newWidth = image.Width <= maxSideSize ? image.Width : maxSideSize;
            int newHeight = image.Width > maxSideSize ? Convert.ToInt32(image.Height * (maxSideSize / (double)image.Width)) : image.Height;
            //Create the new image based on the new dimensions
            Bitmap newImage = new Bitmap(image, newWidth, newHeight);

            //If the image needs to be square
            if (makeItSquare)
            {
                //Get the smaller side
                int smallerSide = newWidth >= newHeight ? newHeight : newWidth;

                double coeficient = maxSideSize / (double)smallerSide;
                //scale the height and width
                newWidth = Convert.ToInt32(coeficient * newWidth);
                newHeight = Convert.ToInt32(coeficient * newHeight);
                //set the image to a temp image using the new height and width
                Bitmap tempImage = new Bitmap(image, newWidth, newHeight);
                //get the cropping values
                int cropX = (newWidth - maxSideSize) / 2;
                int cropY = (newHeight - maxSideSize) / 2;
                //set newimage to a basic bitmap of max size
                newImage = new Bitmap(maxSideSize, maxSideSize);
                //Create a graphic from the basic bitmap
                Graphics tempGraphic = Graphics.FromImage(newImage);
                tempGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //draw the new image onto the bimap and crop
                tempGraphic.DrawImage(tempImage, new Rectangle(0, 0, maxSideSize, maxSideSize), cropX, cropY, maxSideSize, maxSideSize, GraphicsUnit.Pixel);
            }


            //save the new image
            newImage.Save(server.MapPath(filePath), ImageFormat.Png);

            //Dispose of the images
            image.Dispose();
            newImage.Dispose();
        }
    }

    public class JPPDirectory
    {
        public enum ImageTypes
        {
            AchievementIcon,
            QuestIcon,
            News,
            SiteContent,
            ProfilePicture,
            ContentSubmission,
            UserStory,
            UserQRCode
        }

        public static void CheckAndCreateNewsDirectory(HttpServerUtilityBase serverUtilityBase)
        {
            HttpServerUtilityBase server = serverUtilityBase;

            string serverPath = server.MapPath("~");
            string imagesPath = serverPath + "Content\\Images";

            if (!Directory.Exists(imagesPath + "\\News"))
                Directory.CreateDirectory(imagesPath + "\\News");
        }

        public static void CheckAndCreateSiteContentDirectory(HttpServerUtilityBase serverUtilityBase)
        {

            HttpServerUtilityBase server = serverUtilityBase;

            string serverPath = server.MapPath("~");
            string imagesPath = serverPath + "Content\\Images";

            if (!Directory.Exists(imagesPath + "\\SiteContent"))
                Directory.CreateDirectory(imagesPath + "\\SiteContent");
        }

        public static void CheckAndCreateUserDirectory(int userID, HttpServerUtilityBase serverUtilityBase)
        {
            HttpServerUtilityBase server = serverUtilityBase;

            string serverPath = server.MapPath("~");
            string imagesPath = serverPath + "Content\\Images\\Users";
            string userDirectory = imagesPath + "\\" + userID.ToString();

            if (!Directory.Exists(userDirectory))
                Directory.CreateDirectory(userDirectory);

            if (!Directory.Exists(userDirectory + "\\ProfilePictures"))
                Directory.CreateDirectory(userDirectory + "\\ProfilePictures");

            if (!Directory.Exists(userDirectory + "\\ContentSubmissions"))
                Directory.CreateDirectory(userDirectory + "\\ContentSubmissions");

            if (!Directory.Exists(userDirectory + "\\UserStories"))
                Directory.CreateDirectory(userDirectory + "\\UserStories");

            if (!Directory.Exists(userDirectory + "\\UserQRCodes"))
                Directory.CreateDirectory(userDirectory + "\\UserQRCodes");
        }

        public static void CheckAndCreateAchievementAndQuestDirectory(HttpServerUtilityBase serverUtilityBase)
        {
            HttpServerUtilityBase server = serverUtilityBase;

            string serverPath = server.MapPath("~");
            string imagesPath = serverPath + "Content\\Images";

            if (!Directory.Exists(imagesPath + "\\Achievements"))
                Directory.CreateDirectory(imagesPath + "\\Achievements");

            if (!Directory.Exists(imagesPath + "\\Quests"))
                Directory.CreateDirectory(imagesPath + "\\Quests");

        }

        /// <summary>
        /// Create the file path that uploaded images will be saved to.
        /// </summary>
        /// <param name="serverUtilityBase"></param>
        /// <param name="imageType"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static string CreateFilePath(ImageTypes imageType, int? userID = null)
        {
            string filePath = "~/Content/Images";
            string fileName = Guid.NewGuid().ToString();

            switch (imageType)
            {
                case ImageTypes.AchievementIcon:

                    filePath += "/Achievements/" + fileName + ".png";
                    break;

                case ImageTypes.QuestIcon:

                    filePath += "/Quests/" + fileName + ".png";
                    break;

                case ImageTypes.News:

                    filePath += "/News/" + fileName + ".png";
                    break;

                case ImageTypes.SiteContent:

                    filePath += "/SiteContent/" + fileName + ".png";
                    break;

                // Cases for User Directories (Must include a userID or else the file path will be an empty string.
                case ImageTypes.ProfilePicture:

                    if (userID != null)
                        filePath += "/Users/" + userID.ToString() + "/ProfilePictures/" + fileName + ".png";
                    else
                        filePath = "";

                    break;

                case ImageTypes.UserQRCode:

                    if (userID != null)
                        filePath += "/Users/" + userID.ToString() + "/UserQRCodes/" + fileName + ".png";
                    else
                        filePath = "";

                    break;

                case ImageTypes.ContentSubmission:

                    if (userID != null)
                        filePath += "/Users/" + userID.ToString() + "/ContentSubmissions/" + fileName + ".png";
                    else
                        filePath = "";

                    break;

                case ImageTypes.UserStory:

                    if (userID != null)
                        filePath += "/Users/" + userID.ToString() + "/UserStories/" + fileName + ".png";
                    else
                        filePath = "";

                    break;

                default:
                    filePath = "";
                    break;
            }

            return filePath;
        }
    }

    public static class JppUriInfo
    {
        /// <summary>
        /// Gets the current website's domain name.
        /// </summary>
        /// <param name="request">An HTTP request object (in controllers, this is just Request)</param>
        /// <returns>The current domain URI</returns>
        public static string GetCurrentDomain(HttpRequestBase request)
        {
            return request.Url.Scheme + System.Uri.SchemeDelimiter + request.Url.Host + (request.Url.IsDefaultPort ? "" : ":" + request.Url.Port);
        }

        /// <summary>
        /// Converts a relative URI to an absolute one (i.e. replaces ~ with domain name)
        /// </summary>
        /// <param name="request">An HTTP request object</param>
        /// <param name="relativePath">The relative path to convert</param>
        /// <returns>The full path, e.g. domain.com/relativePath</returns>
        public static string GetAbsoluteUri(HttpRequestBase request, string relativePath)
        {
            return GetCurrentDomain(request) + VirtualPathUtility.ToAbsolute(relativePath);
        }
    }

    public static class JppFacebookHelper
    {
        public static string GetAppAccessToken(Facebook.FacebookClient fbClient = null)
        {
            // TODO: Cache in DB via site settings rather than fetching every time?
            if (fbClient == null)
            {
                fbClient = new Facebook.FacebookClient();
            }

            string appId = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppId);
            string appSecret = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppSecret);
            object appAccessTokenParams = new { client_id = appId, client_secret = appSecret, grant_type = "client_credentials" };
            dynamic appAccessTokenObject = fbClient.Get("/oauth/access_token", appAccessTokenParams);
            string appAccessToken = appAccessTokenObject.access_token;

            return appAccessToken;
        }
    }

    public class JPPSendGrid
    {
        public class JPPSendGridProperties
        {
            public String fromEmail { get; set; }
            public List<String> toEmail { get; set; }
            public List<String> ccEmail { get; set; }
            public List<String> bccEmail { get; set; }
            public String subjectEmail { get; set; }
            public String htmlEmail { get; set; }
            public String textEmail { get; set; }
        }

        public static void SendEmail(JPPSendGridProperties properties)
        {
            SendGrid newEmail = SendGrid.GetInstance();

            //From
            newEmail.From = new MailAddress(properties.fromEmail, "Just Press Play");
            //To
            foreach (String recipient in properties.toEmail)
                newEmail.AddTo(recipient);
            //CC
            // foreach (String recipient in properties.ccEmail)
            //   newEmail.AddTo(recipient);
            //BCC
            // foreach (String recipient in properties.bccEmail)
            //  newEmail.AddTo(recipient);
            //Subject
            newEmail.Subject = properties.subjectEmail;
            //Html
            newEmail.Html = properties.htmlEmail;
            //Text
            // newEmail.Text = properties.textEmail;

            //Credentials
            var credentials = new NetworkCredential(JPPConstants.SendGridUserName, JPPConstants.SendGridPassword);

            //Create a REST transport for sending email.
            var transportREST = Web.GetInstance(credentials);

            //Send the email.
            transportREST.Deliver(newEmail);
        }
    }

}







