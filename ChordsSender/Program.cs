using Newtonsoft.Json;
using System.Net;
using IronPdf;
using System.Security.Permissions;
using System.Diagnostics;
using PuppeteerSharp;
using System.Net.NetworkInformation;
using PuppeteerSharp.Input;

namespace ChordsSender
{
    public class Program
    {
        public static AppSettings? Settings { get; set; }
        static void Main(string[] args)
        {
            Console.WriteLine("Chords Sheet Sender");
            Console.WriteLine($"(c) {DateTime.Now.Year} Aebibtech");

            var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fileName = "chordssheetsender.json";
            var settingsFilePath = Path.Combine(userProfilePath, fileName);

            if (File.Exists(settingsFilePath))
            {
                var file = File.ReadAllText(settingsFilePath);
                Settings = JsonConvert.DeserializeObject<AppSettings>(file);
            }
            else
            {
                string chordsPath = "";
                while (string.IsNullOrEmpty(chordsPath))
                {
                    Console.WriteLine("Chords Path:");
                    chordsPath = Console.ReadLine();
                }
                string fbUsername = "";
                while (string.IsNullOrEmpty(fbUsername))
                {
                    Console.WriteLine("FB Username:");
                    fbUsername = Console.ReadLine();
                }
                string fbPassword = "";
                while (string.IsNullOrEmpty(fbPassword))
                {
                    Console.WriteLine("FB Password:");
                    fbPassword = Console.ReadLine();
                }
                string threadId = "";
                while (string.IsNullOrEmpty(threadId))
                {
                    Console.WriteLine("GC Thread ID:");
                    threadId = Console.ReadLine();
                }
                Settings = new AppSettings() { PDfPath = chordsPath, FBUsername = fbUsername, FBPassword = fbPassword, ThreadID = threadId };
                var settingsJson = JsonConvert.SerializeObject(Settings);
                File.WriteAllText(settingsFilePath, settingsJson);
            }

            var chordsToSend = GetMergedPdf();
            SendChordsFile(chordsToSend, $"https://www.facebook.com/messages/t/{Settings.ThreadID}").GetAwaiter().GetResult();
        }

        public static async Task SendChordsFile(string chordsFilePath, string gcURL)
        {
            using var pinger = new Ping();
            PingReply reply = await pinger.SendPingAsync("google.com");

            if (reply is not { Status: IPStatus.Success })
            {
                Console.WriteLine("Internet is down... Aborting");
                System.Environment.Exit(1);
            }

            Console.WriteLine("Internet is good! Proceeding...");

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var googleChrome = "Google\\Chrome\\Application\\chrome.exe";

            Console.WriteLine($"Using Chrome from {Path.Combine(programFiles, googleChrome)}");

            try
            {
                var userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"ChordsSenderProfile");
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                {
                    Headless = true,
                    ExecutablePath = Path.Combine(programFiles, googleChrome),
                    UserDataDir = userDataDir
                });

                var gcPage = await browser.NewPageAsync();
                await gcPage.SetViewportAsync(new ViewPortOptions()
                {
                    Height = 600,
                    Width = 1366
                });
                await gcPage.GoToAsync(gcURL);

                var delayedPress = new TypeOptions() { Delay = 100 };

                var usernameInput = await gcPage.QuerySelectorAsync("input#email");
                IElementHandle passwordInput;
                IElementHandle loginButton;
                if (usernameInput != null)
                {
                    Console.WriteLine($"Logging in with Facebook credentials...");
                    await usernameInput.TypeAsync(Settings.FBUsername, delayedPress);

                    passwordInput = await gcPage.QuerySelectorAsync("input#pass");
                    await passwordInput.TypeAsync(Settings.FBPassword, delayedPress);

                    loginButton = await gcPage.QuerySelectorAsync("button#loginbutton");
                    await loginButton.ClickAsync();
                    //await gcPage.ScreenshotAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ChordsSender" + DateTime.Now.ToString("yyyymmdd_HHmmss") + "_1FBLogin.jpg"));
                }

                Console.WriteLine($"Uploading chords PDF file {chordsFilePath}");
                var fileInputSel = "input[type=\"file\"]";
                await gcPage.WaitForSelectorAsync(fileInputSel);
                //await gcPage.ScreenshotAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ChordsSender" + DateTime.Now.ToString("yyyymmdd_HHmmss") + "_2GC.jpg"));
                var fileUpload = await gcPage.QuerySelectorAsync(fileInputSel);
                await fileUpload.UploadFileAsync(new string[] { chordsFilePath });

                var greeting = GetGreeting();
                Console.WriteLine($"Typing greeting '{greeting}'");
                var messageBox = await gcPage.XPathAsync("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div/div[4]/div[2]/div/div/div[1]");
                await messageBox[0].TypeAsync(greeting, delayedPress);
                //await gcPage.ScreenshotAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ChordsSender" + DateTime.Now.ToString("yyyymmdd_HHmmss") + "_3AfterInput.jpg"));

                Console.WriteLine("Sending chords to GC...");
                var sendButton = await gcPage.XPathAsync("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/span[2]/div");
                await sendButton[0].ClickAsync();
                //await gcPage.ScreenshotAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "ChordsSender" + DateTime.Now.ToString("yyyymmdd_HHmmss") + "_4MessageSent.jpg"));

                await browser.CloseAsync();

                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something happened: {ex.Message}\n");
                System.Environment.Exit(1);
            }
        }

        static string GetMergedPdf()
        {
            var latestChordsPath = Directory.EnumerateDirectories(Settings.PDfPath).Last();
            var dateFileName = latestChordsPath.Split('\\').Last();
            var outputFileName = dateFileName + "_chords.pdf";
            var files = Directory.EnumerateFiles(Path.Combine(Settings.PDfPath, latestChordsPath));
            var outputPath = Path.Combine(Settings.PDfPath, outputFileName);
            var pdfFiles = new List<PdfDocument>();

            foreach (var file in files)
            {
                pdfFiles.Add(new PdfDocument(file));
            }

            var chordsFile = PdfDocument.Merge(pdfFiles);
            chordsFile.SaveAs(outputPath);

            return outputPath;
        }

        static string GetGreeting()
        {
            var hour = DateTime.Now.Hour;
            var greeting = "Good {} po team. Ito po ang ating chords para po ngayong Linggo. See you all! :)";
            if (hour >= 0 && hour <= 11)
            {
                greeting = greeting.Replace("{}", "morning");
            }
            if (hour >= 12 && hour <= 17)
            {
                greeting = greeting.Replace("{}", "afternoon");
            }
            if (hour >= 18 && hour <= 23)
            {
                greeting = greeting.Replace("{}", "evening");
            }
            return greeting;
        }
    }
}