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

namespace LeagueAppWF
{
    class Stats
    {
        // creates dictionaries then computes stats about the searched for players winrates
        public static List<winInfo> doStats(string SummonerName)
        {
            leagueInfo._sem.WaitOne();
            int SummonerID = leagueInfo.id;
            string path = ConfigurationManager.AppSettings.Get("path");
            leaguedata.leagueDictionaries dicts = new leaguedata.leagueDictionaries();
            Dictionary<int, List<string>> matchIDtoName = dicts.getMatchIDtoName(SummonerName);
            Dictionary<int, string> matchIDtoWin = dicts.getMatchIDtoWin(SummonerName, SummonerID);
            Dictionary<int, string> CIDtoCName = dicts.getCIDtoCName();
            Dictionary<int, string> matchIDtoCName = dicts.getMatchIDtoCName(dicts.getMatchList(SummonerName), CIDtoCName);
            Dictionary<string, int> playerOccurences = new Dictionary<string, int>();
            Dictionary<string, int> winsWithPlayer = new Dictionary<string, int>();

            foreach (KeyValuePair<int, List<string>> kvp in matchIDtoName)
            {
                foreach(string player in kvp.Value)
                {
                    if (!playerOccurences.ContainsKey(player))
                    {
                        playerOccurences.Add(player, 1);
                    }
                    else
                    {
                        playerOccurences[player]++;
                    }
                }
            }

            var itemsToRemove = playerOccurences.Where(f => f.Value < 4).ToArray();
            foreach (var item in itemsToRemove)
            {
                playerOccurences.Remove(item.Key);
            }


            foreach (KeyValuePair<int, List<string>> kvp in matchIDtoName)
            {
                foreach (string player in kvp.Value)
                {
                    if (playerOccurences.ContainsKey(player) && matchIDtoWin[kvp.Key] == "won")
                    {
                        if (winsWithPlayer.ContainsKey(player))
                        {
                            winsWithPlayer[player]++;
                        }
                        else
                        {
                            winsWithPlayer.Add(player, 1);
                        }
                    }
                }
            }

            System.IO.StreamWriter winInfoFile = new System.IO.StreamWriter(path + "winInfo" + SummonerName + ".txt");
            List<winInfo> finalStats = new List<winInfo>();
            foreach (KeyValuePair<string, int> kvp in playerOccurences)
            {
                int wins = 0;
                winInfo w = new winInfo();
                if (winsWithPlayer.ContainsKey(kvp.Key))
                {
                    wins = winsWithPlayer[kvp.Key];
                    w.wins = winsWithPlayer[kvp.Key];

                }
                int games = kvp.Value;
                w.games = kvp.Value;
                int winPercent = 100 * wins / games;
                w.winPercent = 100 * w.wins / w.games;
                w.name = kvp.Key;
                winInfoFile.WriteLine(kvp.Key + " wins: " + wins + " Games: " + kvp.Value + " win%: " + winPercent);
                finalStats.Add(w);
            }
            winInfoFile.Close();
            leagueInfo._sem.Release();
            return finalStats;
        }
    }

    public class winInfo
    {
        public string name;
        public int wins;
        public int games;
        public int winPercent;
    }
}
