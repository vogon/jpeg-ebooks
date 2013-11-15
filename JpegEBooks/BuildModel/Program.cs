using JpegEBooks.ImageModel;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BuildModel
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            if (args.Length < 1)
            {
                Console.WriteLine("pass in a filename, ya jerk");
            }
            else
            {
                Bitmap bmp = new Bitmap(args[0]);
                MarkovModel mm = new MarkovModel(2, Neighborhoods.VonNeumann);

                mm.LoadFromImage(bmp);
            }
		}
	}
}
