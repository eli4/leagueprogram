using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Collections.Specialized;

namespace LeagueApp
{
    class leagueInfo
    {
        async static void DoJSON()
        {
            string apikey = ConfigurationManager.AppSettings.Get("apikey");
            string nameinfo = ConfigurationManager.AppSettings.Get("name");
            string path = ConfigurationManager.AppSettings.Get("path");
            var client = new HttpClient();

            // Get the response.
            Stream nameInfo = await client.GetStreamAsync("https://na.api.pvp.net/api/lol/na/v1.4/summoner/by-name/" + nameinfo + "?api_key=" + apikey);

            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, Summoner>), settings);
            Dictionary<string, Summoner> user = (Dictionary<string, Summoner>)serializer.ReadObject(nameInfo);

            string key = user.Keys.First<string>();
            int id = user[key].id;
            string name = user[key].name;



            string rankAPI = "https://na.api.pvp.net/api/lol/na/v2.2/matchlist/by-summoner/" + id.ToString() + "?api_key=" + apikey;
            Stream rankHistory = await client.GetStreamAsync(rankAPI);
            DataContractJsonSerializer serializer2 = new DataContractJsonSerializer(typeof(BaseMatch));
            BaseMatch history = (BaseMatch)serializer2.ReadObject(rankHistory);

            Dictionary<int, string> idToChampName = new Dictionary<int, string>();

            string champAPI = "https://na.api.pvp.net/api/lol/static-data/na/v1.2/champion?api_key=" + apikey;
            Stream champInfo = await client.GetStreamAsync(champAPI);
            DataContractJsonSerializer serializer3 = new DataContractJsonSerializer(typeof(BaseChampion), settings);
            BaseChampion champ = (BaseChampion)serializer3.ReadObject(champInfo);
            champ.GenerateNameLookup();
            Console.WriteLine(champ.data["Brand"]);

            foreach ( Match m in history.matches)
            {
                //Dictionary<int, List<string>> matchSummoners = new Dictionary<int, List<string>>();
                int champID = m.champion;
                Console.WriteLine(champID);
                m.name = champ.nameLookup[champID];

                string text = System.IO.File.ReadAllText(path + "matchs" + name + ".txt");

                if (string.IsNullOrEmpty(text) || text.Contains(m.matchId.ToString()) == false) {
                    string matchAPI = "https://na.api.pvp.net/api/lol/na/v2.2/match/" + m.matchId.ToString() + "?api_key=" + apikey;
                    Stream matchDetailStream = await client.GetStreamAsync(matchAPI);
                    DataContractJsonSerializer serializer4 = new DataContractJsonSerializer(typeof(MatchDetail));
                    MatchDetail matchDetail = (MatchDetail)serializer4.ReadObject(matchDetailStream);

                    System.IO.StreamWriter file = File.AppendText(path + "matchs" + name + ".txt");
                    file.WriteLine("MatchId: " + m.matchId.ToString());
                    file.WriteLine("Champion " + name + " played: " + m.name);
                    file.WriteLine("Players in match: " + matchDetail.participantIdentities[0].player.summonerName + ", " +
                                   matchDetail.participantIdentities[1].player.summonerName + ", " +
                                   matchDetail.participantIdentities[2].player.summonerName + ", " +
                                   matchDetail.participantIdentities[3].player.summonerName + ", " +
                                   matchDetail.participantIdentities[4].player.summonerName + ", " +
                                   matchDetail.participantIdentities[5].player.summonerName + ", " +
                                   matchDetail.participantIdentities[6].player.summonerName + ", " +
                                   matchDetail.participantIdentities[7].player.summonerName + ", " +
                                   matchDetail.participantIdentities[8].player.summonerName + ", " +
                                   matchDetail.participantIdentities[9].player.summonerName
                    );
                    //for(int i = 0; i<10; i++)
                    //{
                    //    if (matchDetail.participantIdentities[i].player.summonerName == name)
                    //    {
                    //        if (matchDetail.participantIdentities[i].winner == true)
                    //            file.WriteLine(name + " won");
                    //        else
                    //            file.WriteLine(name + " lost");
                    //    }
                    //}
                    file.WriteLine();
                    file.Close();
                }

            
               // Console.WriteLine(m.name);
               // Console.WriteLine(matchDetail.participantIdentities[1].player.summonerName);
                //Thread.Sleep(1000);
            }

        }

        static void Main(string[] args)
        {
            DoJSON();
            Console.ReadLine();
        }

    }

    public class Summoner
    {
        public int id { get; set; }
        public string name { get; set; }
        public int profileIconId { get; set; }
        public int summonerLevel { get; set; }
        public long revisionDate { get; set; }
    }


    [DataContract]
    public class Match
    {
        [DataMember]
        public string region { get; set; }
        [DataMember]
        public string platformId { get; set; }
        [DataMember]
        public int matchId { get; set; }
        [DataMember]
        public int champion { get; set; }
        [DataMember]
        public string queue { get; set; }
        [DataMember]
        public string season { get; set; }
        [DataMember]
        public object timestamp { get; set; }
        [DataMember]
        public string lane { get; set; }
        [DataMember]
        public string role { get; set; }

        public string name;
    }

    [DataContract]
    public class BaseMatch
    {
        [DataMember]
        public List<Match> matches { get; set; }
        [DataMember]
        public int startIndex { get; set; }
        [DataMember]
        public int endIndex { get; set; }
        [DataMember]
        public int totalGames { get; set; }
    }

    public class Champion
    {
        public int id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string title { get; set; }
    }

    [DataContract]
    public class BaseChampion
    {
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public Dictionary<string, Champion> data;

        public Dictionary<int, string> nameLookup;

        public void GenerateNameLookup()
        {
            nameLookup = data.ToDictionary(kv => kv.Value.id, kv => kv.Key);
        }

    }

    public class MatchDetail
    {
        public List<ParticipantIdentity> participantIdentities;
        public List<Participant> participants;
    }
    public class Participant
    {
        public ParticipantStats stats;
    }
    public class ParticipantStats
    {
        Boolean winner;
    }
    public class ParticipantIdentity
    {
        public Player player;
    }

    public class Player
    {
        public string summonerName;
    }
}
