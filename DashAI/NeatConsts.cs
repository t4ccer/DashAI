using System;
using System.Collections.Generic;
using System.Linq;

namespace DashAI
{
    public static class NeatConsts
    {
        public const int ViewX = 7;
        public const int ViewY = 7;
        public const int LogRate = 5;
        public const int SpecCount = 150;
        public const int TileSize = 16;
        public const string experimentName = @"output/foobar";
        public static List<int> typeIds = new List<int>();
        public const bool RecordPlay = false;
        public const string MapName = "Map.bmp";
        static NeatConsts()
        {
            foreach (int item in Enum.GetValues(typeof(TileType)))
            {
                if(item!=0)
                    typeIds.Add(item);
            }
        }

    }
}
