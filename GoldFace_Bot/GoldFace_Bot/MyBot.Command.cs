using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
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
        private static readonly char[] cSeparator = new char[] { ' ' };

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
        private Stack<Stream> mTempStreamStack = new Stack<Stream>(2 ^ 4);
        private async void AnimeCapture(Message message)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            if (DirectoryCheck(ANIME_CAPTURE_FILE_PATH) == false) { SendErrorMessage(message); return; };

            string filePath = string.Empty;
            string fileName = string.Empty;
            string recvMessage = message.Text;

            try
            {
                string[] lStringArr = recvMessage.Split(cSeparator, StringSplitOptions.None);
                int lCount = 1;
                if (lStringArr.Length >= 2)
                {
                    if (Int32.TryParse(lStringArr[1], out lCount) == false)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Invalid Argument. (Argument Only integer.)");
                        return;
                    }
                }

                if (lCount > 10)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Too many request. (max:10)");
                    return;
                }

                IAlbumInputMedia[] inputMedia = new IAlbumInputMedia[lCount];
                for (int iCount = 0; iCount < lCount; ++iCount)
                {
                    filePath = RandFilePath(IMAGE_TYPE.ANIME_CAPTURE);
                    fileName = Path.GetFileName(filePath);

                    Stream stream = System.IO.File.OpenRead(filePath);
                    mTempStreamStack.Push(stream);
                    inputMedia[iCount] = new InputMediaPhoto(new InputMedia(stream, fileName));
                }

                await Bot.SendMediaGroupAsync(inputMedia, message.Chat.Id);

                while (mTempStreamStack.Count > 0)
                {
                    Stream stream = mTempStreamStack.Pop();
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (System.Exception e)
            {
                while (mTempStreamStack.Count > 0)
                {
                    Stream stream = mTempStreamStack.Pop();
                    stream.Close();
                    stream.Dispose();
                }

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
/bankall - all user bank account.
/anime - show today anime list.
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
            string recvMessage = message.Text;

            try
            {
                string[] lStringArr = recvMessage.Split(cSeparator, StringSplitOptions.None);
                int lCount = 1;
                if (lStringArr.Length >= 2)
                {
                    if (Int32.TryParse(lStringArr[1], out lCount) == false)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Invalid Argument. (Argument Only integer.)");
                        return;
                    }
                }

                if (lCount > 10)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Too many request. (max:10)");
                    return;
                }

                IAlbumInputMedia[] inputMedia = new IAlbumInputMedia[lCount];
                for (int iCount = 0; iCount < lCount; ++iCount)
                {
                    filePath = RandFilePath(IMAGE_TYPE.PUBLIC_ILLUST);
                    fileName = Path.GetFileName(filePath);

                    Stream stream = System.IO.File.OpenRead(filePath);
                    mTempStreamStack.Push(stream);
                    inputMedia[iCount] = new InputMediaPhoto(new InputMedia(stream, fileName));
                }

                await Bot.SendMediaGroupAsync(inputMedia, message.Chat.Id);

                while (mTempStreamStack.Count > 0)
                {
                    Stream stream = mTempStreamStack.Pop();
                    stream.Close();
                    stream.Dispose();
                }

            }
            catch (System.Exception e)
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
            for (int iUser = 0; iUser < userNodes.Count; ++iUser)
            {
                if (userNodes[iUser].Attributes["Name"].Value.Equals(username))
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

        #region ## /anime ##

        private async void TodayAnimeInfo(Message message)
        {
            string[] result = message.Text.Split(' ');
            int dayOfWeek = (int)DateTime.Now.DayOfWeek;
            if (result.Length >= 2)
            {
                if (Int32.TryParse(result[1], out dayOfWeek) == false)
                    dayOfWeek = ConvertDayToNumber.Convert(result[1]);
            }
            string responseText = String.Empty;
            string url = $"{BotConfig.instance.Config.AnimeDataUrl}{dayOfWeek}";
            Console.WriteLine($"{BotConfig.instance.Config.AnimeDataUrl}{dayOfWeek}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8;";
            request.Timeout = 30 * 1000; // 30sec
            request.UserAgent = "Chrome/xyz";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode status = response.StatusCode;
                Console.WriteLine(status);

                Stream responseStream = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    responseText = sr.ReadToEnd();
                }
            }

            List<AnissiaAnimeList> animeList = _ParseJsonToAnimeList(responseText);

            if (animeList == null)
                responseText = "[Error] Response is null or Empty";

            StringBuilder sb = new StringBuilder(1024);
            foreach (var anime in animeList)
            {
                sb.AppendLine($"{anime.ToString()}");
            }

            responseText = sb.ToString();

            await Bot.SendTextMessageAsync(message.Chat.Id, responseText);
        }

        private List<AnissiaAnimeList> _ParseJsonToAnimeList(string responseJson)
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                Console.WriteLine("[Error] Response is null or Empty");
                return null;
            }

            return JsonConvert.DeserializeObject<List<AnissiaAnimeList>>(responseJson);
        }

        #endregion
    }
}
