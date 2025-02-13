﻿using PuppeteerSharp.Input;
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
                Headless = false,
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
                //var messageBox = await gcPage.WaitForXPathAsync("/html/body/div[1]/div/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div/div/div/div[1]/div/div[2]/div/div/div/div[2]/div/div/div[4]/div[2]/div/div[1]/div[1]");
                var messageBox = await gcPage.WaitForXPathAsync("/html/body/div[1]/div/div[1]/div/div[3]/div/div/div[1]/div[1]/div[2]/div/div/div/div[1]/div/div/div/div[1]/div/div[2]/div/div/div/div[2]/div/div/div[4]/div[2]/div/div[1]/div[1]");
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
