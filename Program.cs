using System;
using System.Collections.Generic;
using System.IO;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Logger;
using InstagramApiSharp;
using System.Threading.Tasks;
using RedditSharp.Things;
using RedditSharp;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Timers;
using InstagramApiSharp.Classes.Android.DeviceInfo;

namespace rareinsults_bot
{
    class Program
    {
        #region Hidden

        private const string username = "me_irl_bot_two";
        private const string password = "------";

        #endregion

        private static UserSessionData user;
        private static IInstaApi api;
        private static Timer aTimer;

        public static void Main(string[] args)
        {
            user = new UserSessionData
            {
                UserName = username,
                Password = password
            };

            var botVersion = "3.0";

            Boolean testMode = false;

            if (testMode == true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  ***********************************************************************************");
                Console.WriteLine("\n  *             me_irl_bot_two for Instagram, version " + botVersion + ", by @tylastrog           *");
                Console.Write("\n  *");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("                R U N N I N G      I N      T E S T      M O D E                 ");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("*\n");
                Console.WriteLine("\n  ***********************************************************************************");
                Console.WriteLine("\n ");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  ***********************************************************************************");
                Console.WriteLine("\n  *             me_irl_bot_two for Instagram, version " + botVersion + ", by @tylastrog           *");
                Console.WriteLine("\n  ***********************************************************************************");
                Console.WriteLine("\n ");
                Console.ResetColor();
            }

            Login();
            Console.ReadLine();
        }

        public static async void Login()
        {
            var device = new AndroidDevice
            {
                AndroidBoardName = "msm8996",
                AndroidBootloader = "G935TUVU3APG1",
                DeviceBrand = "samsung",
                DeviceModel = "SM-G935T",
                DeviceModelBoot = "qcom",
                DeviceModelIdentifier = "hero2qltetmo",
                FirmwareBrand = "hero2qltetmo",
                FirmwareFingerprint ="samsung/hero2qltetmo/hero2qltetmo:6.0.1/MMB29M/G935TUVU3APG1:user/release-keys",
                FirmwareTags = "release-keys",
                FirmwareType = "user",
                HardwareManufacturer = "samsung",
                HardwareModel = "SM-G935T",
                DeviceGuid = Guid.NewGuid(),
                PhoneGuid = Guid.NewGuid(),
                Resolution = "1440x2560",
                Dpi = "640dpi"
            };

            var delay = RequestDelay.FromSeconds(2, 2);

            api = InstaApiBuilder.CreateBuilder()
             .SetUser(user)
             .UseLogger(new DebugLogger(LogLevel.Exceptions))
             .SetRequestDelay(delay)
             .Build();

            api.SetDevice(device);

            var loginRequest = await api.LoginAsync();
            Program p = new Program();

            if (loginRequest.Succeeded)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("** SUCCESS:");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" Logged into user, ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("@" + username);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("!\n");
                Console.ResetColor();

                UserInformation(username);

                await p.DownloadImageFromReddit();

                aTimer = new System.Timers.Timer((60 * 60 * 1000) / 2);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.Enabled = true;
            }
            else
            {
                ErrorReason("Error", loginRequest.Info.Message);
            }
        }

        public static async void Logout()
        {
            var device = new AndroidDevice
            {
                AndroidBoardName = "msm8996",
                AndroidBootloader = "G935TUVU3APG1",
                DeviceBrand = "samsung",
                DeviceModel = "SM-G935T",
                DeviceModelBoot = "qcom",
                DeviceModelIdentifier = "hero2qltetmo",
                FirmwareBrand = "hero2qltetmo",
                FirmwareFingerprint = "samsung/hero2qltetmo/hero2qltetmo:6.0.1/MMB29M/G935TUVU3APG1:user/release-keys",
                FirmwareTags = "release-keys",
                FirmwareType = "user",
                HardwareManufacturer = "samsung",
                HardwareModel = "SM-G935T",
                DeviceGuid = Guid.NewGuid(),
                PhoneGuid = Guid.NewGuid(),
                Resolution = "1440x2560",
                Dpi = "640dpi"
            };

            api.SetDevice(device);
            var logoutRequest = await api.LogoutAsync();
            Success("Logged out.");
        }

        public static async void UserInformation(string userToScrape)
        {
            var currentUser = await api.UserProcessor.GetCurrentUserAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
             $"\n\tNAME: {currentUser.Value.FullName} " +
             $"\n\tUSERNAME: @{currentUser.Value.UserName} " +
             $"\n\tWEBSITE: {currentUser.Value.ExternalUrl} " +
             $"\n\tEMAIL: {currentUser.Value.Email} " +
             $"\n\tVERIFIED: {currentUser.Value.IsVerified}\n");
            Console.ResetColor();
        }

        string imageFolder = @"C:\dev\me_irl_bot_two\images\";
        string gifFolder = @"C:\dev\me_irl_bot_two\gif\";

         private static void OnTimedEvent(Object source, ElapsedEventArgs e)
         {
            Program p = new Program();

            try
            {
                p.DownloadImageFromReddit();
            }
            catch (IOException ex)
            {
                ErrorReason("", ex.Message);
            }
         }

        public static void DownloadImage(string imageURL, string userDir)
        {
            StartTime();

            Process("Searching for post to download.");

            var filename = imageURL.Split('/').Last();

            Downloading(imageURL);

            WebClient client = new WebClient();
            client.DownloadFile(imageURL, userDir + filename);

            Result($"\n\n\tDOWNLOADED: " + filename + ".");
        }

        public async Task DownloadImageFromReddit()
        {
            Reddit reddit = new Reddit();
            var subreddit = reddit.GetSubreddit("me_irl");

            int i = 2;
        restart:
            foreach (var post in subreddit.Hot.Skip(i).Take(1))
            {
                var lines = File.ReadAllLines(@"C:\dev\me_irl_bot_two\credit\done.txt");
                
                if (!lines.Contains(post.Shortlink))
                {
                    if (post.IsStickied || post.IsSelfPost || Convert.ToString(post.Url).Contains("reddituploads")) continue;
                        string postURL = Convert.ToString(post.Url);
                        
                        if (postURL.Contains(".png") || postURL.Contains(".jpg") || postURL.Contains(".jpeg"))
                        {
                            File.AppendAllText(@"C:\dev\me_irl_bot_two\credit\done.txt", post.Shortlink + Environment.NewLine);

                            DownloadImage(postURL, imageFolder);

                            File.WriteAllText(@"C:\dev\me_irl_bot_two\credit\author.txt", post.Author.FullName);
                            File.WriteAllText(@"C:\dev\me_irl_bot_two\credit\title.txt", post.Title);
                            File.WriteAllText(@"C:\dev\me_irl_bot_two\credit\link.txt", post.Shortlink);

                            await ConvertImage();
                    }
                    else
                    {
                        Process("Media was not an image.");
                        i++;
                        goto restart;
                    }
                }
                else
                {
                    i++;
                    goto restart;
                }
            }
        }

        public async Task ConvertImage()
        {
            string[] filePaths = Directory.GetFiles(imageFolder);

            foreach (string myfile in filePaths)
            {
                Process("Converting image for Instagram.");

                try
                {
                    Image bmpImageToConvert = Image.FromFile(myfile);
                    Image bmpNewImage = new Bitmap(bmpImageToConvert.Width, bmpImageToConvert.Height);
                    Graphics gfxNewImage = Graphics.FromImage(bmpNewImage);

                    gfxNewImage.DrawImage(bmpImageToConvert, new Rectangle(0, 0, bmpNewImage.Width, bmpNewImage.Height), 0, 0, bmpImageToConvert.Width, bmpImageToConvert.Height, GraphicsUnit.Pixel);
                    gfxNewImage.Dispose();
                    bmpImageToConvert.Dispose();

                    bmpNewImage.Save(imageFolder + "bot.jpg", ImageFormat.Jpeg);

                    await resizeImage();
                }
                catch (OutOfMemoryException ex)
                {
                    ErrorReason("", ex.Message);
                }
            }
        }

        public async Task resizeImage()
        {
            Image image = Image.FromFile(imageFolder + "bot.jpg");

            int Width = 1080;
            int Height = 1080;

            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt32((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt32((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(image, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

            bmPhoto.Save(imageFolder + "bot_resized.jpg", ImageFormat.Jpeg);
            image.Dispose();

            #region Old Code.
            //Image image = Image.FromFile(imageFolder + "bot.jpg");

            //int canvasWidth = 1080;
            //int canvasHeight = 1080;
            //int orignalWidth = image.Width;
            //int orignalHeight = image.Height;

            //System.Drawing.Image thumbnail = new Bitmap(canvasWidth, canvasHeight);
            //System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(thumbnail);

            //graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //graphic.SmoothingMode = SmoothingMode.HighQuality;
            //graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //graphic.CompositingQuality = CompositingQuality.HighQuality;

            //double ratioX = (double)canvasWidth / (double)orignalWidth;
            //double ratioY = (double)canvasHeight / (double)orignalHeight;

            //double ratio = ratioX < ratioY ? ratioX : ratioY;

            //int newWidth = Convert.ToInt32((canvasWidth * ratio));
            //int newHeight = Convert.ToInt32((canvasHeight * ratio));

            //int posX = Convert.ToInt32((canvasWidth - (orignalWidth * ratio)) / 2);
            //int posY = Convert.ToInt32((canvasHeight - (orignalHeight * ratio)) / 2);

            //graphic.Clear(Color.White);
            //graphic.DrawImage(image, posX, posY, newWidth, newHeight);

            //System.Drawing.Imaging.ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();

            //EncoderParameters encoderParameters;
            //encoderParameters = new EncoderParameters(1);
            //encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

            //thumbnail.Save(imageFolder + "bot_resized.jpg", info[1], encoderParameters);
            //image.Dispose();
        #endregion

            await UploadAndComment();
        }

        public async Task UploadAndComment()
        {
            string postAuthor = File.ReadAllText(@"C:\dev\me_irl_bot_two\credit\author.txt");
            string postTitle = File.ReadAllText(@"C:\dev\me_irl_bot_two\credit\title.txt");
            string postLink = File.ReadAllText(@"C:\dev\me_irl_bot_two\credit\link.txt");

            string location = imageFolder + "bot_resized.jpg";
            string caption = postTitle;
            if (caption.ToLower() == "me irl" || caption.ToLower() == "me_irl")
            {
                caption = "me_irl";
            }
            string imageUser = "/u/" + postAuthor;
            string comment = "A mirrored post from /r/me_irl by " + imageUser + ": " + postLink;
            string url = "http://instagram.com/p/";

            var mediaImage = new InstaImageUpload
            {
                Height = 0,
                Width = 0,
                Uri = @location
            };

            Process("Uploading image to Instagram and adding caption and credit.");

            var result = await api.MediaProcessor.UploadPhotoAsync(mediaImage, "" + caption);
            var commentResult = await api.CommentProcessor.CommentMediaAsync(result.Value.Pk, comment);

            if (result.Succeeded && commentResult.Succeeded)
            {
                Success("Media was uploaded.");
                Result(
                 $"\n\n\tTYPE: " + result.Value.MediaType +
                 $"\n\tCAPTION: \"" + caption + "\"" +
                 $"\n\tID: " + result.Value.Pk +
                 $"\n\tLINK: " + url + result.Value.Code +
                 $"\n\tSOURCE: " + location +
                 $"\n\tCREDIT: \"" + comment + "\"");

                try
                {
                    await DeleteImages();
                } catch (Exception ex)
                {
                    ErrorReason("Error detected. ", ex.Message);

                    EndTime();

                    Wait();
                }
            }
            else
            {

                if (!result.Succeeded)
                {
                    Error("Media could not be uploaded.");

                    Reason(result.Info.Message);

                    var deleteMedia = await api.MediaProcessor.DeleteMediaAsync(result.Value.Pk, result.Value.MediaType);
                    Logout();

                    Environment.Exit(0);
                }
                else if (!commentResult.Succeeded)
                {
                    Error("Media could not be captioned.");

                    Reason(commentResult.Info.Message);
                    var deleteMedia = await api.MediaProcessor.DeleteMediaAsync(result.Value.Pk, result.Value.MediaType);
                    Logout();
                    Environment.Exit(0);
                }
                else if (!commentResult.Succeeded && !result.Succeeded)
                {
                    Error("Media could not be captioned or uploaded.");

                    Reason(commentResult.Info.Message);
                    Reason(result.Info.Message);
                    var deleteMedia = await api.MediaProcessor.DeleteMediaAsync(result.Value.Pk, result.Value.MediaType);

                    Logout();
                    Environment.Exit(0);
                }
            }
        }

        public async Task DeleteImages()
        {
            DirectoryInfo di = new DirectoryInfo(imageFolder);

            Process("Clearing the download folder for next image.");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            File.Delete(@"C:\dev\me_irl_bot_two\credit\author.txt");
            File.Delete(@"C:\dev\me_irl_bot_two\credit\title.txt");
            File.Delete(@"C:\dev\me_irl_bot_two\credit\link.txt");

            Success("Cleared the folder of images.");

            EndTime();

            Wait();
        }

        #region Console Writers
        public static void Downloading(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** DOWNLOADING:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" \n\n\t" + message + ". \n\n");
            Console.ResetColor();
        }

        public static void EndTime()
        {

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("** END:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(DateTime.Now.ToString(" hh:mm:ss tt\n"));
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** ERROR:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" " + message + "\n\n");
            Console.ResetColor();
        }

        public static void ErrorReason(string message, string reason)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** ERROR:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" " + message);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" (" + reason + ")");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(".\n\n");
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public static void Frozen()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("** CONSOLE IS FROZEN\n");
            Console.ResetColor();
            Console.ReadLine();
        }

        public static void Process(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** PROCESS:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" " + message + "\n\n");
            Console.ResetColor();
        }

        public static void Reason(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** REASON:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" " + message + "\n\n");
            Console.ResetColor();
        }

        public static void Result(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** RESULT:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(" " + message);
            Console.ResetColor();
            Console.Write("\n");
            Console.ResetColor();
        }

        public static void StartTime()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("** START:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(DateTime.Now.ToString(" hh:mm:ss tt\n\n"));
            Console.ResetColor();
        }

        public static void Success(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("** SUCCESS:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" " + message + "\n\n");
            Console.ResetColor();
        }

        public static void Wait()
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n** WAITING...");
            Console.ResetColor();
            Console.Write(" \n\n");
        }
        #endregion
    }
}