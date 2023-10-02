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
            ConsoleHelper.ShowMessage();

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
                    ConsoleHelper.ShowMessage("Chords Path:");
                    chordsPath = Console.ReadLine();
                    if (Directory.Exists(chordsPath))
                    {
                        break;
                    }
                    else
                    {
                        ConsoleHelper.ShowMessage("Invalid directory. Where is your chords directory?");
                        continue;
                    }
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

            var chordsToSend = new PdfMerger(Settings).MergePdf();
            new FacebookSender(Settings).SendChordsFile(chordsToSend, $"https://www.facebook.com/messages/t/{Settings.ThreadID}").GetAwaiter().GetResult();
        }
    }
}