﻿using System;
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
			BotConfig.Init();

			bool lIsLoad = BotConfig.instance.IsInitComplete;
			if(lIsLoad == false)
			{
				Console.WriteLine("[Error] Load 'BotConfig' File Failed!!");
				Console.ReadLine();
				return;
			}

			string token = BotConfig.instance.Config.ApiToken;
			string anime_capture_path = BotConfig.instance.Config.AnimeCaptureFilePath;
			string public_illust_path = BotConfig.instance.Config.PublicIllustFilePath;

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
