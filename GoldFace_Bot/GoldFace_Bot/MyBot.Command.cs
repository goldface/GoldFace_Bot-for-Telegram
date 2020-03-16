﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;

namespace GoldFace_Bot
{
	partial class MyBot
	{
		#region ## /animecaptureinfo ##

		private async void PhotoInfo(Message message)
		{
			var info = @"The total number of images of Bot is ";

			if (DirectoryCheck(ANIME_CAPTURE_FILE_PATH) == false) { SendErrorMessage(message); return; };

			string[] files = Directory.GetFiles(ANIME_CAPTURE_FILE_PATH);
			await Bot.SendTextMessageAsync(message.Chat.Id, info + files.Length.ToString(),
						replyMarkup: null);
		}

		#endregion

		#region ## /animecapture ##

		private async void AnimeCapture(Message message)
		{
			await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

			if (DirectoryCheck(ANIME_CAPTURE_FILE_PATH) == false) { SendErrorMessage(message); return; };

			string filePath = string.Empty;
			string fileName = string.Empty;

			try
			{
				filePath = RandFilePath(IMAGE_TYPE.ANIME_CAPTURE);
				fileName = Path.GetFileName(filePath);

				using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var fts = new InputOnlineFile(fileStream, fileName);

					await Bot.SendPhotoAsync(message.Chat.Id, fts, fileName);
				}
			}
			catch(System.Exception e)
			{
				string log = string.Format("filePath:'{0}', fileName:'{1}'", filePath, fileName);
				Console.WriteLine(log);
				Console.WriteLine("Send Failed: " + e.ToString());
			}

		}

		#endregion

		#region ## /help ##

		private async void Help(Message message)
		{
			var usage = @"Usage:
/help - show command.
/animecapture - send a random anime capture image.
/animecaptureinfo - send a anime capture image total count.
/illust - send a random illust image.
/illustinfo - send a illust image total count.
/bank - author bank account.
/bank - all user bank account.
";

			await Bot.SendTextMessageAsync(message.Chat.Id, usage);
		}

		#endregion

		#region ## /adduser ##

		private async void AddUser(Message message)
		{
			string[] result = message.Text.Split(' ');
			if (result.Length < 3) { SendErrorMessage(message, "Command is not correct."); return; }
			if (AddUserXml(result[1], result[2]) == false) { SendErrorMessage(message, "Add User Failed."); return; }
			string msg = "Add User Success.";
			await Bot.SendTextMessageAsync(message.Chat.Id, msg);
		}

		#endregion

		#region ## /deleteuser ##

		private async void DeleteUser(Message message)
		{
			string[] result = message.Text.Split(' ');
			if (result.Length < 2) { SendErrorMessage(message, "Command is not correct."); return; }
			if (DeleteUserXml(message.From.Username, result[1]) == false) { SendErrorMessage(message, "Delete User Failed."); return; }
			string msg = "Delete User Success.";
			await Bot.SendTextMessageAsync(message.Chat.Id, msg);
		}

		#endregion

		#region ## /illustinfo ##

		private async void Public_IllustInfo(Message message)
		{
			var info = @"The total number of images of Bot is ";

			if (DirectoryCheck(PUBLIC_ILLUST_FILE_PATH) == false) { SendErrorMessage(message); return; };

			string[] files = Directory.GetFiles(PUBLIC_ILLUST_FILE_PATH);
			await Bot.SendTextMessageAsync(message.Chat.Id, info + files.Length.ToString(),
						replyMarkup: null);
		}

		#endregion

		#region ## /illust ##

		private async void Public_Illust(Message message)
		{
			await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

			if (DirectoryCheck(PUBLIC_ILLUST_FILE_PATH) == false) { SendErrorMessage(message); return; };

			string filePath = string.Empty;
			string fileName = string.Empty;

			try
			{
				filePath = RandFilePath(IMAGE_TYPE.PUBLIC_ILLUST);
				fileName = Path.GetFileName(filePath);

				using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var fts = new InputOnlineFile(fileStream, fileName);

					await Bot.SendPhotoAsync(message.Chat.Id, fts, string.Empty);
				}
			}
			catch(System.Exception e)
			{
				string log = string.Format("filePath:'{0}', fileName:'{1}'", filePath, fileName);
				Console.WriteLine(log);
				Console.WriteLine("Send Failed: " + e.ToString());
			}

		}

		#endregion

		#region ## /bank ##

		private async void BankAccountInfo(Message message)
		{
			var bankInfo = BotConfig.instance.Config.BankAccountInfo;

			var username = message.From.Username;
			//string xpath = string.Format(, username);
			XmlNodeList userNodes = xmlUserlist.SelectNodes("//users/user");

			int iUserIdx = 0;
			for(int iUser = 0; iUser < userNodes.Count; ++iUser)
			{
				if(userNodes[iUser].Attributes["Name"].Value.Equals(username))
				{
					iUserIdx = iUser;
					break;
				}
			}

			await Bot.SendTextMessageAsync(message.Chat.Id, bankInfo[iUserIdx]);
		}

		#endregion

		#region ## /bankall ##
		private async void BankAllAccountInfo(Message message)
		{
			var bankInfo = BotConfig.instance.Config.BankAccountInfo;

			StringBuilder sb = new StringBuilder();
			for (int ibank = 0; ibank < bankInfo.Length; ++ibank)
				sb.AppendLine(bankInfo[ibank]);

			await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString());
		}
		#endregion
	}
}
