using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DashAI
{
    public class BmpMap : IMap
    {
        public int[,] map { get; }

        public BmpMap(string path)
        {
            var bmp = new Bitmap(path);
            var groundColor = Color.FromArgb(0, 0, 0);
            var spikesColor = Color.FromArgb(255, 0, 0);
            var jumpOrb = Color.FromArgb(0, 255, 0);
            map = new int[bmp.Height, bmp.Width];


            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    map[y, x] = 
                          (bmp.GetPixel(x, y) == groundColor) ? 1 
                        : (bmp.GetPixel(x, y) == spikesColor) ? 2 
                        : (bmp.GetPixel(x,y) == jumpOrb) ? 3 
                        :0;
                }
            }
        }
    }
}
