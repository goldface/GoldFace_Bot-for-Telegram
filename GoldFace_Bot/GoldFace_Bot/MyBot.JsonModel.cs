using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldFace_Bot
{
    partial class MyBot
    {
        public class AnissiaAnimeList
        {
            public int animeNo { get; set; }        // 애니번호
            public string status { get; set; }         // 방영여부
            public string time { get; set; }        // 시간
            public string subject { get; set; }     // 제목
            public string genres { get; set; }      // 장르
            public int captionCount { get; set; }   // 자막 참여자 수
            public string startDate { get; set; }   // 시작일
            public string endDate { get; set; }     // 종료일
            public string website { get; set; }     // 공식사이트

            public string ToString()
            {
                string lStatus = status.Equals("ON") ? "" : "[결방]";
                string lSubject = subject;
                string lGenres = $"[{genres}]";

                return $"{lStatus}{subject}         {lGenres}";
            }
        }

        public class ConvertDayToNumber
        {
            public static int Convert(string day)
            {
                if(day.StartsWith("일")) { return 0; }
                else if (day.StartsWith("월")) { return 1; }
                else if (day.StartsWith("화")) { return 2; }
                else if (day.StartsWith("수")) { return 3; }
                else if (day.StartsWith("목")) { return 4; }
                else if (day.StartsWith("금")) { return 5; }
                else if (day.StartsWith("토")) { return 6; }
                else if (day.StartsWith("기")) { return 7; }
                else if (day.StartsWith("신")) { return 8; }
                else { return (int) DateTime.Now.DayOfWeek;}
            }    
        }
    }
}
