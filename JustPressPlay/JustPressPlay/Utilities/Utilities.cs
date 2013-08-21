using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace JustPressPlay.Utilities
{

    public class JPPImage
    {
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

		public static void SavePlayerImages(HttpServerUtilityBase server, string filePath, string fileNameNoExt, Stream stream)
		{
			SaveImageAtSize(server, filePath + fileNameNoExt + "_s.png", stream, JPPConstants.ImageSizes.Small);
			SaveImageAtSize(server, filePath + fileNameNoExt + "_m.png", stream, JPPConstants.ImageSizes.Medium);
			SaveImageAtSize(server, filePath + fileNameNoExt + "_l.png", stream, JPPConstants.ImageSizes.Large);
		}

		enum LargerSide { Width, Height, Same };
		public static void SaveImageAtSize(HttpServerUtilityBase server, string filePath, Stream stream, int size)
		{
			// The starting image
			Image originalImage = Image.FromStream(stream);
			
			// New empty image and graphics for manipulation
			Bitmap newImage = new Bitmap(size, size);
			Graphics g = Graphics.FromImage(newImage);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			
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

			// Draw and save
			g.DrawImage(originalImage, offsetWidth, offsetHeight, scaledWidth, scaledHeight);
			newImage.Save(filePath, ImageFormat.Png);
			
			// Cleanup
			originalImage.Dispose();
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
        public static void Save(HttpServerUtilityBase serverUtilityBase, string filePath, Stream stream, int maxSideSize, bool makeItSquare)
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
            UserStory
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
}






  
