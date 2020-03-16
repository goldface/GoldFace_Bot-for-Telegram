using System;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoldFace_Bot
{
    class BotConfig
    {
        public class ConfigJsonInfo
        {
            public string ApiToken { get; set; }
            public string AnimeCaptureFilePath { get; set; }
            public string PublicIllustFilePath { get; set; }
            public string[] BankAccountInfo { get; set; }
        }

        private static readonly string cBotConfigDirectoryName = @"Config";
        private static readonly string cBotConfigJsonFilePath = @"Config/BotConfig.json";
        private static BotConfig mBotConfig;
        public static BotConfig instance
        {
            get
            {
                if (mBotConfig == null)
                    Console.WriteLine("[Error] BotConfig Init() First.");

                return mBotConfig;
            }
        }

        public ConfigJsonInfo Config { get; private set; }

        public bool IsInitComplete { get; private set; }

        public static void Init()
        {
            if (mBotConfig != null)
            {
                Console.WriteLine("Init() Call twice.");
                return;
            }

            mBotConfig = new BotConfig();
            mBotConfig.IsInitComplete = mBotConfig._Load();
            if(mBotConfig.IsInitComplete == false)
            {
                mBotConfig._AutoCreateConfigJsonFile();
                Console.WriteLine("AutoCreate BotConfigJsonFile.");
            }
        }

        private bool _Load()
        {
            JsonSerializer lJsonSerializer = new JsonSerializer();

            if (File.Exists(cBotConfigJsonFilePath) == false)
                return false;

            //JObject lJsonObject = JObject.Parse();
            //string lApiTokenString = lJsonObject.Value<string>("API_TOKEN");

            Config = JsonConvert.DeserializeObject<ConfigJsonInfo>(File.ReadAllText(cBotConfigJsonFilePath));
            return true;
        }

        private void _AutoCreateConfigJsonFile()
        {
            ConfigJsonInfo lConfig = new ConfigJsonInfo();
            lConfig.ApiToken = string.Empty;
            lConfig.AnimeCaptureFilePath = string.Empty;
            lConfig.PublicIllustFilePath = string.Empty;
            lConfig.BankAccountInfo = new string[] { "" };

            if (Directory.Exists(cBotConfigDirectoryName) == false)
                Directory.CreateDirectory(cBotConfigDirectoryName);
            
            File.Create(cBotConfigJsonFilePath).Close();

            File.WriteAllText(cBotConfigJsonFilePath, JsonConvert.SerializeObject(lConfig, Formatting.Indented), Encoding.UTF8);
        }
    }
}
