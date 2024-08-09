using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TSEconomy.Configuration.Models
{
    public class BalTopLeaderboard
    {
        public (string, int, string)[] LeaderBoardRanks = { ("First Rank", 3467, Color.IndianRed.Hex3()),
                                                            ("Second Rank", 1552, Color.Goldenrod.Hex3()),
                                                            ("Third Rank", 1006, Color.Gold.Hex3()),
                                                            ("Fourth Rank", 1225, Color.Yellow.Hex3()),
                                                            ("Fifth Rank", 391, Color.YellowGreen.Hex3()),
                                                            ("Sixth Rank", 1184, Color.SeaGreen.Hex3())};

        public bool UseFibonacciRankStyle = true;
        public int LeaderboardPositionsPerRanks = 5;
        public int MaxRanksPerPage = 10;
    }
}
