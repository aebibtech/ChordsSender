using PuppeteerSharp.Input;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static ChordsSender.ConsoleHelper;

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
                ShowMessage("Internet is down... Aborting");
                Environment.Exit(1);
            }

            ShowMessage("Internet is good! Proceeding...");

            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var googleChrome = @"Google\Chrome\Application\chrome.exe";
            var userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"ChordsSenderProfile");
            Console.WriteLine($"Using Chrome from {Path.Combine(programFiles, googleChrome)}");
            IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Headless = true,
                ExecutablePath = Path.Combine(programFiles, googleChrome),
                UserDataDir = userDataDir,
                Timeout = 0
            });
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
                    ShowMessage("Logging in with Facebook credentials...");
                    await usernameInput.TypeAsync(_settings.FBUsername, delayedPress);

                    passwordInput = await gcPage.QuerySelectorAsync("input#pass");
                    await passwordInput.TypeAsync(_settings.FBPassword, delayedPress);

                    loginButton = await gcPage.QuerySelectorAsync("button#loginbutton");
                    if (loginButton == null)
                        loginButton = await gcPage.QuerySelectorAsync("button[type=\"submit\"]");
                    await loginButton.ClickAsync();
                }

                ShowMessage($"Uploading chords PDF file {chordsFilePath}");
                var fileInputSel = "input[type=\"file\"]";
                await gcPage.WaitForSelectorAsync(fileInputSel);
                var fileUpload = await gcPage.QuerySelectorAsync(fileInputSel);
                await fileUpload.UploadFileAsync(new string[] { chordsFilePath });
                

                var greeting = new Greeter().GetGreeting(hour: DateTime.Now.Hour);
                ShowMessage($"Typing greeting '{greeting}'");
                //var messageBox = await gcPage.WaitForSelectorAsync("#mount_0_0_39 > div > div:nth-child(1) > div > div.x9f619.x1n2onr6.x1ja2u2z > div > div > div.x78zum5.xdt5ytf.x1t2pt76.x1n2onr6.x1ja2u2z.x10cihs4 > div.x9f619.x2lah0s.x1nhvcw1.x1qjc9v5.xozqiw3.x1q0g3np.x78zum5.x1iyjqo2.x1t2pt76.x1n2onr6.x1ja2u2z > div.x9f619.x1n2onr6.x1ja2u2z.xdt5ytf.x193iq5w.xeuugli.x1r8uery.x1iyjqo2.xs83m0k.x78zum5.x1t2pt76 > div > div > div > div > div > div > div > div.x9f619.x1ja2u2z.x193iq5w.xeuugli.x1r8uery.x1iyjqo2.xs83m0k.x78zum5.xdt5ytf.x6ikm8r.x10wlt62.x1n2onr6 > div > div.x1uvtmcs.x4k7w5x.x1h91t0o.x1beo9mf.xaigb6o.x12ejxvf.x3igimt.xarpa2k.xedcshv.x1lytzrv.x1t2pt76.x7ja8zs.x1n2onr6.x1qrby5j.x1jfb8zj > div > div > div:nth-child(2) > div > div > div.x78zum5.x1iyjqo2.x6q2ic0 > div.x16sw7j7.x107yiy2.xv8uw2v.x1tfwpuw.x2g32xy.x9f619.x1iyjqo2.xeuugli > div > div > div.xzsf02u.x1a2a7pz.x1n2onr6.x14wi4xw.x1iyjqo2.x1gh3ibb.xisnujt.xeuugli.x1odjw0f.notranslate");
                var messageBox = await gcPage.WaitForXPathAsync("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div[1]/div/div/div/div[1]/div/div[2]/div/div/div[2]/div/div/div[4]/div[2]/div/div/div[1]/p");
                await messageBox.TypeAsync(greeting, delayedPress);

                ShowMessage("Sending chords to GC...");
                await messageBox.FocusAsync();
                await gcPage.Keyboard.PressAsync("Enter");

                for (int i = 10; i >= 1; i--)
                {
                    ShowMessage($"Waiting {i}s to ensure that the message was sent.");
                    await Task.Delay(1000);
                }

                await browser.CloseAsync();

                ShowMessage("Done");
            }
            catch (Exception ex)
            {
                ShowMessage($"Something happened: {ex.Message}\n");
                await browser.CloseAsync();
                Environment.Exit(1);
            }
        }
    }
}
