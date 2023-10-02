using PuppeteerSharp.Input;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ChordsSender
{
    public class FacebookSender
    {
        private AppSettings _settings;

        public FacebookSender(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task SendChordsFile(string chordsFilePath, string gcURL)
        {
            using var pinger = new Ping();
            PingReply reply = await pinger.SendPingAsync("google.com");

            if (reply is not { Status: IPStatus.Success })
            {
                ConsoleHelper.ShowMessage("Internet is down... Aborting");
                System.Environment.Exit(1);
            }

            ConsoleHelper.ShowMessage("Internet is good! Proceeding...");

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var googleChrome = "Google\\Chrome\\Application\\chrome.exe";
            var userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"ChordsSenderProfile");
            Console.WriteLine($"Using Chrome from {Path.Combine(programFiles, googleChrome)}");
            IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true,
                ExecutablePath = Path.Combine(programFiles, googleChrome),
                UserDataDir = userDataDir
            }); ;

            try
            {
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
                    ConsoleHelper.ShowMessage("Logging in with Facebook credentials...");
                    await usernameInput.TypeAsync(_settings.FBUsername, delayedPress);

                    passwordInput = await gcPage.QuerySelectorAsync("input#pass");
                    await passwordInput.TypeAsync(_settings.FBPassword, delayedPress);

                    loginButton = await gcPage.QuerySelectorAsync("button#loginbutton");
                    if (loginButton == null)
                        loginButton = await gcPage.QuerySelectorAsync("button[type=\"submit\"]");
                    await loginButton.ClickAsync();
                }

                ConsoleHelper.ShowMessage($"Uploading chords PDF file {chordsFilePath}");
                var fileInputSel = "input[type=\"file\"]";
                await gcPage.WaitForSelectorAsync(fileInputSel);
                var fileUpload = await gcPage.QuerySelectorAsync(fileInputSel);
                await fileUpload.UploadFileAsync(new string[] { chordsFilePath });

                var greeting = new Greeter().GetGreeting(hour: DateTime.Now.Hour);
                ConsoleHelper.ShowMessage($"Typing greeting '{greeting}'");
                var messageBox = await gcPage.XPathAsync("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div[1]/div[2]/div/div/div[2]/div/div/div[4]/div[2]/div/div/div[1]");
                await messageBox[0].TypeAsync(greeting, delayedPress);

                ConsoleHelper.ShowMessage("Sending chords to GC...");
                await messageBox[0].FocusAsync();
                await gcPage.Keyboard.PressAsync("Enter");

                for(int i = 10; i >= 1; i--)
                {
                    ConsoleHelper.ShowMessage($"Waiting {i}s to ensure that the message was sent.");
                    await Task.Delay(1000);
                }

                await browser.CloseAsync();

                ConsoleHelper.ShowMessage("Done");
            }
            catch (Exception ex)
            {
                ConsoleHelper.ShowMessage($"Something happened: {ex.Message}\n");
                await browser.CloseAsync();
                System.Environment.Exit(1);
            }
        }
    }
}
