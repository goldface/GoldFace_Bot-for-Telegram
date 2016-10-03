using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace GoldFace_Bot
{
	partial class MyBot
	{
		enum USER_TYPE
		{
			Guest = 0,
			Master = 10
		}

		enum IMAGE_TYPE
		{
			ANIME_CAPTURE = 0,
			PUBLIC_ILLUST = 1,
		}

		private readonly TelegramBotClient Bot;
		private readonly string ANIME_CAPTURE_FILE_PATH;
		private readonly string PUBLIC_ILLUST_FILE_PATH;
		private readonly XmlDocument xmlUserlist;

		public MyBot(string token, string anime_capture_path, string public_illust_path)
		{
			Bot = new TelegramBotClient(token);
			ANIME_CAPTURE_FILE_PATH = anime_capture_path;
			PUBLIC_ILLUST_FILE_PATH = public_illust_path;
			xmlUserlist = new XmlDocument();
		}

		public void Start()
		{
			LoadUserlist();

			Bot.OnMessage += BotOnMessageReceived;

			var me = Bot.GetMeAsync().Result;
			Console.Title = me.Username;

			Bot.StartReceiving();
			Console.ReadLine();
			Bot.StopReceiving();
		}

		private void LoadUserlist()
		{
			try
			{
				xmlUserlist.Load("userlist.xml");
			}
			catch (System.IO.FileNotFoundException)
			{
				Console.WriteLine("xml load fail");
				CreateXML();
			}
		}

		private bool CheckUser(string username)
		{
			string xpath = string.Format("//users/user[@Name='{0}']", username);
			XmlNodeList userNodes = xmlUserlist.SelectNodes(xpath);
			return userNodes.Count > 0;
		}

		private bool AddUserXml(string username, string type)
		{
			XmlNode rootNode = xmlUserlist.DocumentElement;
			XmlNode userNode = xmlUserlist.CreateElement("user");
			XmlAttribute attribute = xmlUserlist.CreateAttribute("No");
			XmlNodeList list = xmlUserlist.SelectNodes("//users/user");
			attribute.Value = (list.Count + 1).ToString();
			userNode.Attributes.Append(attribute);
			attribute = xmlUserlist.CreateAttribute("Name");
			attribute.Value = username;
			userNode.Attributes.Append(attribute);
			attribute = xmlUserlist.CreateAttribute("Type");

			int typeNumber = (int)USER_TYPE.Guest;
			string typeName;

			bool check = int.TryParse(type, out typeNumber);
			typeName = Enum.GetName(typeof(USER_TYPE), typeNumber);


			if (check == false || string.IsNullOrEmpty(typeName))
			{
				Console.WriteLine("Add User Failed");
				return false;
			}

			attribute.Value = typeName;
			userNode.Attributes.Append(attribute);
			rootNode.AppendChild(userNode);

			xmlUserlist.Save("userlist.xml");
			Console.WriteLine("xml add success");
			return true;
		}

		private void CreateXML()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode rootNode = xmlDoc.CreateElement("users");
			xmlDoc.AppendChild(rootNode);

			XmlNode userNode = xmlDoc.CreateElement("user");
			XmlAttribute attribute = xmlDoc.CreateAttribute("No");
			attribute.Value = "1";
			userNode.Attributes.Append(attribute);
			attribute = xmlDoc.CreateAttribute("Name");
			attribute.Value = "YOUR_ID";
			userNode.Attributes.Append(attribute);
			attribute = xmlDoc.CreateAttribute("Type");
			attribute.Value = "Master";
			userNode.Attributes.Append(attribute);
			rootNode.AppendChild(userNode);

			xmlDoc.Save("userlist.xml");
			Console.WriteLine("xml create success");
		}

		private bool DeleteUserXml(string sendUserName, string userName)
		{
			string xpathMaster = string.Format("//users/user[@Name='{0}']", sendUserName);
			XmlNode masterNode = xmlUserlist.SelectSingleNode(xpathMaster);

			if (masterNode.Attributes["Type"].Value.Equals(USER_TYPE.Master.ToString()) == false)
			{
				return false;
			}

			string xpath = string.Format("//users/user[@Name='{0}']", userName);
			XmlNode usersNode = xmlUserlist.SelectSingleNode("//users");
			XmlNode delNode = xmlUserlist.SelectSingleNode(xpath);
			if (delNode == null) { return false; }

			usersNode.RemoveChild(delNode);
			xmlUserlist.Save("userlist.xml");
			return true;
		}


		private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
		{
			var message = messageEventArgs.Message;
			if (message == null || message.Type != MessageType.TextMessage) return;

			bool access = await AccessCheck(message);
			if (access == false) { return; }

			if (CommandCheck(message.Text, "/animecaptureinfo"))
			{
				PhotoInfo(message);
			}
			else if (CommandCheck(message.Text, "/animecapture"))
			{
				AnimeCapture(message);
			}
			else if (CommandCheck(message.Text, "/help"))
			{
				Help(message);
			}
			else if (CommandCheck(message.Text, "/adduser"))
			{
				AddUser(message);
			}
			else if (CommandCheck(message.Text, "/deleteuser"))
			{
				DeleteUser(message);
			}
			else if (CommandCheck(message.Text, "/illustinfo"))
			{
				Public_IllustInfo(message);
			}
			else if (CommandCheck(message.Text, "/illust"))
			{
				Public_Illust(message);
			}

			await Task.Delay(100);
		}

		private string RandFilePath(IMAGE_TYPE type)
		{
			string path = GetImagePath(type);
			string[] files = Directory.GetFiles(path);
			int rand_Pic_Number = new Random().Next(1, files.Length - 1);
			return files[rand_Pic_Number];
		}

		private string GetImagePath(IMAGE_TYPE type)
		{
			switch (type)
			{
				case IMAGE_TYPE.ANIME_CAPTURE: { return ANIME_CAPTURE_FILE_PATH; }
				case IMAGE_TYPE.PUBLIC_ILLUST: { return PUBLIC_ILLUST_FILE_PATH; }
				default: { return ANIME_CAPTURE_FILE_PATH; }
			}
		}

		private bool DirectoryCheck(string path)
		{
			return Directory.Exists(path);
		}


		private async Task<bool> AccessCheck(Message message)
		{
			bool access = CheckUser(message.From.Username);

			if (!access)
			{
				Console.WriteLine("[!Warning!] Unauthorized users have access.");
				Console.WriteLine(string.Format("UserName:'{0}'", message.From.Username));
				var msg = @"[!Warning!] Unauthorized users. Please contact master";
				await Bot.SendTextMessageAsync(message.Chat.Id, msg, replyMarkup: new ReplyKeyboardHide());
			}

			return access;
		}

		private bool CommandCheck(string message, string command)
		{
			return message.StartsWith(command) ||
					message.StartsWith(command + "@" + Console.Title);
		}

		private async void SendErrorMessage(Message message, string msg = "")
		{
			var vMsg = string.IsNullOrEmpty(msg) ? @"[!Error!] Sorry. A small problem has occurred." : msg;
			await Bot.SendTextMessageAsync(message.Chat.Id, vMsg, replyMarkup: new ReplyKeyboardHide());
		}
	}
}
