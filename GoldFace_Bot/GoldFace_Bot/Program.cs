using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;


namespace GoldFace_Bot
{
	class Program
	{
		private static MyBot Bot;

		static void Main(string[] args)
		{
			string token = Properties.Settings.Default.MY_BOT_API_TOKEN;
			string anime_capture_path = Properties.Settings.Default.ANIME_CAPTURE_FILE_PATH;
			string public_illust_path = Properties.Settings.Default.PUBLIC_ILLUST_FILE_PATH;

			Bot = new MyBot(token, anime_capture_path, public_illust_path);

			Console.WriteLine(token);
			Console.WriteLine("ANIME_CAPTURE_FILE_PATH: " + anime_capture_path);
			Console.WriteLine("PUBLIC_ILLUST_FILE_PATH: " + public_illust_path);

			Bot.Start();

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
				Console.WriteLine("AppConfig Setting Write Error!");
			}

		}
	}
}
