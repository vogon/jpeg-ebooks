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
                Model m = new Model(2);

                m.LoadImage(bmp);

                Bitmap test = m.GenImage(1, 20);
                test.Save(".\\output.png");
            }
		}
	}
}
