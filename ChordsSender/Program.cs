using Newtonsoft.Json;
using static ChordsSender.ConsoleHelper;

namespace ChordsSender
{
    public class Program
    {
        public static AppSettings? Settings { get; set; }
        static void Main(string[] args)
        {
            ShowMessage();

            var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fileName = "chordssheetsender.json";
            var settingsFilePath = Path.Combine(userProfilePath, fileName);

            if (File.Exists(settingsFilePath))
            {
                var file = File.ReadAllText(settingsFilePath);
                Settings = JsonConvert.DeserializeObject<AppSettings>(file);
                if(args.Length > 0)
                {
                    if (args[0] == "t")
                    {
                        Settings.ThreadID = "aebib";
                    }
                }
            }
            else
            {
                string chordsPath = "";
                while (string.IsNullOrEmpty(chordsPath) || !Directory.Exists(chordsPath))
                {
                    ShowMessage("Chords Path:");
                    chordsPath = Console.ReadLine();
                    if (!Directory.Exists(chordsPath))
                    {
                        ShowError($"Chords Path:\n{chordsPath} is an invalid path");
                    }
                }

                string fbUsername = "";
                while (string.IsNullOrEmpty(fbUsername))
                {
                    ShowMessage("FB Username:");
                    fbUsername = Console.ReadLine();
                    if (string.IsNullOrEmpty(fbUsername)){
                        ShowError("FB Username:\nWe need your FB username");
                    }
                }

                string fbPassword = "";
                while (string.IsNullOrEmpty(fbPassword))
                {
                    ShowMessage("FB Password:");
                    fbPassword = Console.ReadLine();
                    if (string.IsNullOrEmpty(fbPassword))
                    {
                        ShowError("FB Password:\nWe need your FB password");
                    }
                }

                string threadId = "";
                while (string.IsNullOrEmpty(threadId))
                {
                    ShowMessage("GC Thread ID:");
                    threadId = Console.ReadLine();
                    if (string.IsNullOrEmpty(threadId))
                    {
                        ShowError("GC Thread ID:\nWe need your group chat's thread ID");
                    }
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