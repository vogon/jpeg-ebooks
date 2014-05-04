using JpegEBooks.ImageModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

                IFormatter formatter = new BinaryFormatter();
                
                Stream stream = new FileStream(".\\output.model", FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, m);
            }
		}
	}
}
