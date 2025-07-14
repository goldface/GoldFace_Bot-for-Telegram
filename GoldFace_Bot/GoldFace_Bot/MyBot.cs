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
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

        enum ANIME_WEEK
        {
			Sunday = 0,
            Monday,
			Tuesday,
			Wednesday,
			Thursday,
			Friday,
			Saturday,
			etc,
			newly
        }

		private readonly TelegramBotClient Bot;
		private readonly string ANIME_CAPTURE_FILE_PATH;
		private readonly string PUBLIC_ILLUST_FILE_PATH;
		private readonly XmlDocument xmlUserlist;
		private static readonly Random cRandom = new Random(unchecked((int)DateTime.Now.Ticks));
		private Dictionary<string, Func<Message, Task>> mMessagesDict = new Dictionary<string, Func<Message, Task>>();
		
		private MapleApiService  mMapleService; 
		
		public MyBot(string token, string anime_capture_path, string public_illust_path)
		{
			Bot = new TelegramBotClient(token);
			ANIME_CAPTURE_FILE_PATH = anime_capture_path;
			PUBLIC_ILLUST_FILE_PATH = public_illust_path;
			xmlUserlist = new XmlDocument();
		}

		public void Start()
		{
			mMapleService = new MapleApiService(Bot);
			
			LoadUserlist();
			AddCommnadList();
			
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

		private void AddCommnadList()
		{
			mMessagesDict.Add("/animecaptureinfo", PhotoInfo);
			mMessagesDict.Add("/animecapture", AnimeCapture);
			mMessagesDict.Add("/help", Help);
			mMessagesDict.Add("/adduser", AddUser);
			mMessagesDict.Add("/deleteuser", DeleteUser);
			mMessagesDict.Add("/illustinfo", Public_IllustInfo);
			mMessagesDict.Add("/illust", Public_Illust);
			mMessagesDict.Add("/bankall", BankAllAccountInfo);
			mMessagesDict.Add("/bank", BankAccountInfo);
			mMessagesDict.Add("/anime", TodayAnimeInfo);
			
			// maple
			mMessagesDict.Add("/maplecharinfo", mMapleService.SerachCharacterInfo);
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
			if (message == null || message.Type != MessageType.Text) return;

			bool access = await AccessCheck(message);
			if (access == false) { return; }

			string command = CommandProcess(message);
			if (CommandCheck(message.Text, command))
			{
				if(mMessagesDict.ContainsKey(command))
					await mMessagesDict[command](message);
			}

		   await Task.Delay(100);
		}

		// 메시지로 부터 명령어를 추출한다.
		private string CommandProcess(Message message)
		{
			string[] splitspace = message.Text.Split(' ');
			string[] commandArray = splitspace[0].Split('@');
			return commandArray[0];
		}

		private string RandFilePath(IMAGE_TYPE type)
		{
			string path = GetImagePath(type);
			string[] files = Directory.GetFiles(path);
			int rand_Pic_Number = cRandom.Next(0, files.Length - 1);
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
				await Bot.SendTextMessageAsync(message.Chat.Id, msg);
			}

			return access;
		}

		//명령어가 유효한지 체크한다.
		private bool CommandCheck(string message, string command)
		{
			return message.StartsWith(command) || message.StartsWith(command + "@" + Console.Title);
		}

		private async void SendErrorMessage(Message message, string msg = "")
		{
			var vMsg = string.IsNullOrEmpty(msg) ? @"[!Error!] Sorry. A small problem has occurred." : msg;
			//await Bot.SendTextMessageAsync(message.Chat.Id, vMsg, replyMarkup: new ReplyKeyboardHide());
			await Bot.SendTextMessageAsync(message.Chat.Id, vMsg);
		}
	}
}
