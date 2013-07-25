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
    public class Utilities
    {
       
    }

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
            ProfilePicture,
            ContentSubmission,
            UserStory
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

                    filePath += "/Quest/" + fileName + ".png";
                    break;
                
                // Cases for User Directories (Must include a userID or else the file path will be an empty string.
                case ImageTypes.ProfilePicture:

                    if (userID != null)
                        filePath += "\\Users\\" + userID.ToString() + "\\ProfilePictures\\" + fileName + ".png";
                    else
                        filePath = "";

                    break;

                case ImageTypes.ContentSubmission:

                    if (userID != null)
                        filePath += "\\Users\\" + userID.ToString() + "\\ContentSubmissions\\" + fileName + ".png";
                    else
                        filePath = "";

                    break;

                case ImageTypes.UserStory:

                    if (userID != null)
                        filePath += "\\Users\\" + userID.ToString() + "\\UserStories\\" + fileName + ".png";
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
}






  
