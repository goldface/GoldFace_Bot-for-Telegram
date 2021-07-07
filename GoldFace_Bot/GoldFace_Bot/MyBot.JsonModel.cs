using System;
using System.Collections.Generic;
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
    }
}
