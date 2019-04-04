using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PoETradeAlert
{
    class Watcher
    {
        private long lastReadLength;
        private string file;
        private static readonly HttpClient client = new HttpClient();

        private string pushoverAppToken;
        private string pushoverUserToken;

        public Watcher(string clientTxt, string appToken, string userToken)
        {
            file = clientTxt;
            pushoverAppToken = appToken;
            pushoverUserToken = userToken;
        }

        public void Start()
        {
            //lastReadLength = new FileInfo(file).Length;
            lastReadLength = 0;

            Console.WriteLine("lastReadLength: {0}", lastReadLength);

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = file.Replace("Client.txt", "");

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                // Only watch text files.
                watcher.Filter = "Client.txt";

                // Add event handlers.
                watcher.Changed += OnChanged;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit.");
                while (Console.Read() != 'q') ;
            }
        }

        private void Changes()
        {
            try
            {
                var fileSize = new FileInfo(file).Length;

                Console.WriteLine(lastReadLength);

                if (fileSize > lastReadLength)
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(lastReadLength, SeekOrigin.Begin);
                        var buffer = new byte[1024];

                        var bytesRead = fs.Read(buffer, 0, buffer.Length);
                        lastReadLength += bytesRead;


                        var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        using (var reader = new StringReader(text))
                        {
                            for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                            {
                                // Check for trade whispers
                                if (line.Contains(" @From "))
                                {
                                    var whisper = ParseWhisper(line);

                                    if (whisper != null)
                                    {
                                        SendNotification(whisper);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private Whisper ParseWhisper(string line)
        {
            Regex poeTradeRegex = new Regex("@(.*) (.*): Hi, I would like to buy your (.*) listed for (.*) in (.*) [(]stash tab \"(.*)[\"]; position: left ([0-9]*), top ([0-9]*)[)](.*)");
            Regex poeTradeNoLocationRegex = new Regex("@(.*) (.*): Hi, I would like to buy your (.*) listed for (.*) in (.*)");
            Regex poeTradeUnpricedRegex = new Regex("@(.*) (.*): Hi, I would like to buy your (.*) in (.*) [(]stash tab \"(.*)[\"]; position: left ([0-9]*), top ([0-9]*)[)](.*)");
            Regex poeTradeCurrencyRegex = new Regex("@(.*) (.*): Hi, I'd like to buy your (.*) for my (.*) in (.*).(.*)");

            Regex poeAppRegEx = new Regex("@(.*) (.*): wtb (.*) listed for (.*) in (.*) [(]stash \"(.*)[\"]; left ([0-9]*), top ([0-9]*)[)](.*)");
            Regex poeAppUnpricedRegex = new Regex("@(.*) (.*): wtb (.*) in (.*) [(]stash \"(.*)[\"]; left ([0-9]*), top ([0-9]*)[)](.*)");
            Regex poeAppCurrencyRegex = new Regex("@(.*) (.*): I'd like to buy your (.*) for my (.*) in (.*).(.*)");

            if (poeTradeRegex.IsMatch(line))
            {
                MatchCollection matches = Regex.Matches(line, poeTradeRegex.ToString());

                var whisper = new Whisper();

                foreach (Match match in matches)
                {
                    whisper.Item = match.Groups[3].Value;
                    whisper.Price = match.Groups[4].Value;
                    whisper.Stash = match.Groups[6].Value;
                }

                return whisper;
            }

            return null;
        }

        private async void SendNotification(Whisper whisper)
        {
            var json = JsonConvert.SerializeObject(whisper);

            Console.WriteLine(json);

            var values = new Dictionary<string, string>
            {
                { "token", pushoverAppToken },
                { "user", pushoverUserToken },
                { "message", whisper.Format() },
                { "html", "1" },
            };

            var content = new FormUrlEncodedContent(values);

            await client.PostAsync("https://api.pushover.net/1/messages.json", content);
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            Changes();
        }
    }
}
