using System;
using System.Collections.Specialized;
using System.Configuration;

namespace PoETradeAlert
{
    static class Settings
    {
        public static NameValueCollection ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        if (string.IsNullOrWhiteSpace(appSettings[key]))
                        {
                            Console.WriteLine("{0} is not set, please provide a value", key);

                            string input = Console.ReadLine();

                            AddUpdateAppSettings(key, input);
                        }
                        else
                        {
                            Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                        }
                    }
                }

                return appSettings;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");

                return null;
            }
        }

        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }
    }
}
