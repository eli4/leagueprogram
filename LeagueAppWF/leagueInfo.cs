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
using leaguedata;

namespace LeagueAppWF
{
    class leagueInfo
    {
        public static int id;

        //gets the summoner name from the search form and finds the corresponding ID 
        //then calls to create the 3 api files
        static async Task<bool> DoJSON(string summonerName)
        {
            _sem.WaitOne();
            string apikey = ConfigurationManager.AppSettings.Get("apikey");
            string nameinfo = summonerName;
            string path = ConfigurationManager.AppSettings.Get("path");
            var client = new HttpClient();

            Stream nameInfo = await client.GetStreamAsync("https://na.api.pvp.net/api/lol/na/v1.4/summoner/by-name/" + nameinfo + "?api_key=" + apikey);
            Thread.Sleep(1100);

            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer userser = new DataContractJsonSerializer(typeof(Dictionary<string, Summoner>), settings);
            Dictionary<string, Summoner> userDict = (Dictionary<string, Summoner>)userser.ReadObject(nameInfo);

            string key = userDict.Keys.First<string>();
            id = userDict[key].id;

            leaguedata.LocalFileGen files = new leaguedata.LocalFileGen();
            if (!File.Exists(path + "champs.txt"))
            {
                int x = await files.ChampInfoFile();
            }
            if (!File.Exists(path + "history" + nameinfo + ".txt"))
            {
                int x = await files.MatchHistoryFile(nameinfo, id);
            }
            leaguedata.leagueDictionaries dicts = new leaguedata.leagueDictionaries();
            Dictionary<int, string> champIDtoName = dicts.getCIDtoCName();
            List<Match> matches = dicts.getMatchList(nameinfo);
            int y = await files.MatchInfoFile(nameinfo, matches, champIDtoName);
            _sem.Release();
            return true;
        }

       public static Semaphore _sem;

       public async static Task<List<winInfo>> Core(string SummonerName)
       {
            _sem = new Semaphore(0, 1);
            _sem.Release();
            await DoJSON(SummonerName);
            List<winInfo> x = Stats.doStats(SummonerName);
            return x;
       }

    }
}
