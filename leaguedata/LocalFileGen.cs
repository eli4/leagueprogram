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

namespace leaguedata
{
    // handles all api requests and stores the responses into files
    public class LocalFileGen
    {
        public async Task<int> MatchHistoryFile(string SummonerName, int id)
        {
            string apikey = ConfigurationManager.AppSettings.Get("apikey");
            string path = ConfigurationManager.AppSettings.Get("path");
            var client = new HttpClient();
            string rankAPI = "https://na.api.pvp.net/api/lol/na/v2.2/matchlist/by-summoner/" + id.ToString() + "?api_key=" + apikey;
            Stream rankHistory = await client.GetStreamAsync(rankAPI);
            Thread.Sleep(1100);
            DataContractJsonSerializer historySer = new DataContractJsonSerializer(typeof(BaseMatch));
            BaseMatch historyDetail = (BaseMatch)historySer.ReadObject(rankHistory);
            MemoryStream historyStream = new MemoryStream();
            historySer.WriteObject(historyStream, historyDetail);
            historyStream.Position = 0;
            StreamReader historyJSON = new StreamReader(historyStream);
            System.IO.StreamWriter historyFile = File.AppendText(path + "history" + SummonerName + ".txt");
            historyFile.WriteLine(historyJSON.ReadToEnd());
            historyFile.Close();
            return 1;

        }
        public async Task<int> ChampInfoFile()
        {
            string apikey = ConfigurationManager.AppSettings.Get("apikey");
            string path = ConfigurationManager.AppSettings.Get("path");
            var client = new HttpClient();
            string champAPI = "https://na.api.pvp.net/api/lol/static-data/na/v1.2/champion?api_key=" + apikey;
            Stream champInfo = await client.GetStreamAsync(champAPI);
            Thread.Sleep(1100);
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer champSer = new DataContractJsonSerializer(typeof(BaseChampion), settings);
            BaseChampion champ = (BaseChampion)champSer.ReadObject(champInfo);
            champ.GenerateNameLookup();
            MemoryStream champStream = new MemoryStream();
            champSer.WriteObject(champStream, champ);
            champStream.Position = 0;
            StreamReader championJSON = new StreamReader(champStream);
            System.IO.StreamWriter champFile = File.AppendText(path + "champs.txt");
            champFile.WriteLine(championJSON.ReadToEnd());
            champFile.Close();
            return 1;
        }

        public async Task<int> MatchInfoFile(string SummonerName, List<Match> matches, Dictionary<int, string> champIDtoName)
        {
            string apikey = ConfigurationManager.AppSettings.Get("apikey");
            string path = ConfigurationManager.AppSettings.Get("path");
            var client = new HttpClient();
            foreach (Match m in matches)
            {
                MatchDetail matchDetail = new MatchDetail();
                int champID = m.champion;
                m.name = champIDtoName[champID];

                string text = "";

                if (File.Exists(path + "matchs" + SummonerName + ".txt"))
                {
                    text = System.IO.File.ReadAllText(path + "matchs" + SummonerName + ".txt");
                }

                if (text.Contains(m.matchId.ToString()) == false)
                {

                    string matchAPI = "https://na.api.pvp.net/api/lol/na/v2.2/match/" + m.matchId.ToString() + "?api_key=" + apikey;

                    bool matchDetailsRead = false;
                    while (!matchDetailsRead)
                    {
                        HttpResponseMessage resp = await client.GetAsync(matchAPI);
                        Thread.Sleep(1100);
                        if (resp.IsSuccessStatusCode)
                        {
                            Stream matchDetailStream = await resp.Content.ReadAsStreamAsync();
                            DataContractJsonSerializer serializer4 = new DataContractJsonSerializer(typeof(MatchDetail));
                            matchDetail = (MatchDetail)serializer4.ReadObject(matchDetailStream);
                            matchDetailsRead = true;
                        }
                        else
                        {
                            int waitTime = 0;
                            if (resp != null && resp.Headers != null && resp.Headers.RetryAfter != null)
                            {
                                waitTime = resp.Headers.RetryAfter.Delta.HasValue ? resp.Headers.RetryAfter.Delta.Value.Milliseconds : 0;
                            }
                            Console.WriteLine("Slow Down");
                            Thread.Sleep(waitTime);
                        }
                    }
                    matchDetail.matchID = m.matchId;
                    MemoryStream matchStream = new MemoryStream();
                    DataContractJsonSerializer matchSer = new DataContractJsonSerializer(typeof(MatchDetail));
                    matchSer.WriteObject(matchStream, matchDetail);
                    matchStream.Position = 0;
                    StreamReader matchJSON = new StreamReader(matchStream);

                    System.IO.StreamWriter matchInfoFile = File.AppendText(path + "matchs" + SummonerName + ".txt");
                    matchInfoFile.WriteLine(matchJSON.ReadToEnd());
                    matchInfoFile.Close();

                }
            }
            return 1;
        }
    }

    // reads from the files containing api responses and returns dictionaries with relevent information
    public class leagueDictionaries
    {

        public Dictionary<int, List<string>> getMatchIDtoName(string SummonerName)
        {
            string path = ConfigurationManager.AppSettings.Get("path");
            Dictionary<int, List<string>> matchIDtoName = new Dictionary<int, List<string>>();
            System.IO.StreamReader matchsFile = new System.IO.StreamReader(path + "matchs" + SummonerName + ".txt");
            int matchID;
            string matchs;

            while ((matchs = matchsFile.ReadLine()) != null)
            {
                MemoryStream matchStream = new MemoryStream(Encoding.UTF8.GetBytes(matchs));
                DataContractJsonSerializer matchser = new DataContractJsonSerializer(typeof(MatchDetail));
                MatchDetail matchDetail = (MatchDetail)matchser.ReadObject(matchStream);
                matchID = matchDetail.matchID;
                List<string> players = new List<string>();
                foreach (ParticipantIdentity p in matchDetail.participantIdentities)
                {
                    players.Add(p.player.summonerName);
                }
                matchIDtoName.Add(matchID, players);
            }
            return matchIDtoName;
        }

        public Dictionary<int, string> getMatchIDtoWin(string SummonerName, int id)
        {
            string path = ConfigurationManager.AppSettings.Get("path");
            Dictionary<int, string> matchIDtoWin = new Dictionary<int, string>();
            System.IO.StreamReader matchsFile = new System.IO.StreamReader(path + "matchs" + SummonerName + ".txt");
            int matchID;
            string matchs;

            while ((matchs = matchsFile.ReadLine()) != null)
            {
                MemoryStream matchStream = new MemoryStream(Encoding.UTF8.GetBytes(matchs));
                DataContractJsonSerializer matchser = new DataContractJsonSerializer(typeof(MatchDetail));
                MatchDetail matchDetail = (MatchDetail)matchser.ReadObject(matchStream);
                matchID = matchDetail.matchID;

                int? myParticipantId = null;
                int? myTeamId = null;
                for (int i = 0; i < matchDetail.participantIdentities.Count; i++)
                {
                    if (matchDetail.participantIdentities[i].player.summonerId == id)
                    {
                        myParticipantId = matchDetail.participantIdentities[i].participantId;
                        break;
                    }
                }
                for (int j = 0; j < matchDetail.participantIdentities.Count; j++)
                {
                    if (matchDetail.participants[j].participantId == myParticipantId)
                    {
                        myTeamId = matchDetail.participants[j].teamId;
                    }
                }
                foreach (Team t in matchDetail.teams)
                {
                    if (myTeamId == t.teamId)
                    {
                        if (t.winner == true)
                        {
                            matchIDtoWin.Add(matchID, "won");
                        }
                        else
                        {
                            matchIDtoWin.Add(matchID, "lost");
                        }
                    }
                }
            }
            return matchIDtoWin;
        }

        public Dictionary<int, string> getCIDtoCName()
        {
            string path = ConfigurationManager.AppSettings.Get("path");
            string champs = System.IO.File.ReadAllText(path + "champs.txt");
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            MemoryStream champStream = new MemoryStream(Encoding.UTF8.GetBytes(champs));
            DataContractJsonSerializer champser = new DataContractJsonSerializer(typeof(BaseChampion), settings);
            BaseChampion champDetail = (BaseChampion)champser.ReadObject(champStream);
            champDetail.GenerateNameLookup();
            return champDetail.nameLookup;
        }

        public List<Match> getMatchList(string SummonerName)
        {
            string path = ConfigurationManager.AppSettings.Get("path");
            string history = System.IO.File.ReadAllText(path + "history" + SummonerName + ".txt");
            MemoryStream historyStream = new MemoryStream(Encoding.UTF8.GetBytes(history));
            DataContractJsonSerializer historyser = new DataContractJsonSerializer(typeof(BaseMatch));
            BaseMatch historyDetail = (BaseMatch)historyser.ReadObject(historyStream);
            return historyDetail.matches;
        }

        public Dictionary<int, string> getMatchIDtoCName(List<Match> matches, Dictionary<int, string> CIDtoCName)
        {
            Dictionary<int, string> matchIDtoCName = new Dictionary<int, string>();
            foreach (Match m in matches)
            {
                string champion = CIDtoCName[m.champion];
                matchIDtoCName.Add(m.matchId, champion);
            }
            return matchIDtoCName;
        }
    }

    // all classes below this line are used for reading into json objects
    [DataContract]
    public class MatchDetail
    {
        [DataMember]
        public List<ParticipantIdentity> participantIdentities;
        [DataMember]
        public List<Participant> participants;
        [DataMember]
        public List<Team> teams;
        [DataMember]
        public int matchID;
    }
    public class Participant
    {
        public int participantId;
        public int teamId;

    }
    public class Team
    {
        public Boolean winner;
        public int teamId;
    }
    public class ParticipantIdentity
    {
        public Player player;
        public int participantId;
    }

    public class Player
    {
        public string summonerName;
        public long summonerId;
    }

    public class Summoner
    {
        public int id { get; set; }
        public string name { get; set; }
        public int profileIconId { get; set; }
        public int summonerLevel { get; set; }
        public long revisionDate { get; set; }
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
}
