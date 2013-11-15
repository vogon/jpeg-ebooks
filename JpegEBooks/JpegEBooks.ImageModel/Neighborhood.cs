using System;
using System.Collections.Generic;
using System.Drawing;

namespace JpegEBooks.ImageModel
{
	public delegate bool[,] NeighborhoodDelegate(int range);

	public static class Neighborhoods 
	{
		public static bool[,] VonNeumann(int range)
		{
            bool[,] n = (bool[,])Array.CreateInstance(
                typeof(bool), 
                new int[] { 2 * range + 1, 2 * range + 1 }, 
                new int[] { -range, -range }
                );

            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    // make sure Manhattan distance to center is <= range
                    if (Math.Abs(x + y) <= range)
                    {
                        n[x, y] = true;
                    }
                }
            }

            return n;
		}
	}
}

