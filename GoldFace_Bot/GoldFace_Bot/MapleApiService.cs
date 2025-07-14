using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GoldFace_Bot
{
    
    public class MapleApiService
    {
        private string API_KEY =
            "test_e7d3d3b88bf3a4f576b07cf0055f877340aaa95952f4a0ef688c1391c5a0ef13efe8d04e6d233bd35cf2fabdeb93fb0d";
        private string BASE_URL = "https://open.api.nexon.com/";

        private TelegramBotClient BotClient;
        
        public MapleApiService(TelegramBotClient botClient)
        {
            BotClient = botClient;
        }
        
        public async Task<Maple_OCID> SearchCharacterOCID(Message message)
        {
            string[] messageArray = message.Text.Split(' ');
            if (messageArray.Length < 2)
            {
                await BotClient.SendTextMessageAsync(message.Chat.Id, "캐릭터 이름을 입력해주세요");
                return null;
            }
                
            Maple_OCID mapleOCID = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-nxopen-api-key", API_KEY);
                string characterName = messageArray[1];
                HttpResponseMessage response = await client.GetAsync(BASE_URL + $"maplestory/v1/id?character_name={characterName}");
                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);
                mapleOCID = JsonConvert.DeserializeObject<Maple_OCID>(result);
            }

            return mapleOCID;
        }

        public async Task SerachCharacterInfo(Message message)
        {
            Maple_OCID mapleOCID = await SearchCharacterOCID(message);
            if (mapleOCID == null)
                return;
            //Console.WriteLine(DateTime.Today.ToString("yyyy-MM-dd"));
            
            Maple_CharacterBasicInfo mapleCharacterBasicInfo = null;
            string result;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-nxopen-api-key", API_KEY);

                string RequestURL = BASE_URL + $"maplestory/v1/character/basic?ocid={mapleOCID.ocid}";
                Console.WriteLine(RequestURL);
                HttpResponseMessage response = await client.GetAsync(RequestURL);
                result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);
                
                mapleCharacterBasicInfo = JsonConvert.DeserializeObject<Maple_CharacterBasicInfo>(result);
            }

            await BotClient.SendTextMessageAsync(message.Chat.Id, result);
        }
    }
}