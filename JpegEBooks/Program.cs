using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using JpegEBooks.ImageModel;

namespace JpegEBooks
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            if (args.Length < 1)
            {
                Console.WriteLine("pass in a model, ya jerk");
            }
            else
            {
                int w = (args.Length >= 2) ? int.Parse(args[1]) : 100;
                int h = (args.Length >= 3) ? int.Parse(args[2]) : 50;

                IFormatter formatter = new BinaryFormatter();
                Stream model = new FileStream(args[0], FileMode.Open, FileAccess.Read);

                Model m = (Model)formatter.Deserialize(model);

                Bitmap bmp = m.GenImage(w, h);
                bmp.Save(".\\output.bmp");
            }
		}
	}
}
