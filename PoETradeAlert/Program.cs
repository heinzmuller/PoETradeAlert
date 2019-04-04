using System;
using System.IO;
using System.Linq;

namespace PoETradeAlert
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientTxt = args.Any() ? args[0] : null;

            if (string.IsNullOrWhiteSpace(clientTxt))
            {
                clientTxt = FindClientTxt();
            }

            Console.WriteLine(clientTxt);

            if (!string.IsNullOrWhiteSpace(clientTxt))
            {
                var settings = Settings.ReadAllSettings();

                var watcher = new Watcher(clientTxt, settings.Get("PushoverAppToken"), settings.Get("PushoverUserToken"));

                watcher.Start();
            }
            else
            {
                Console.WriteLine("Could not find Client.txt - Exiting");
            }
        }

        static string FindClientTxt()
        {
            string[] paths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Grinding Gear Games\Path of Exile\logs\Client.txt"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Grinding Gear Games\Path of Exile\logs\Client.txt"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Steam\steamapps\common\Path of Exile\logs\Client.txt"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Steam\steamapps\common\Path of Exile\logs\Client.txt"),
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
