using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace JpegEBooks.ImageModel
{
	public class MarkovModel
	{
		public MarkovModel(int n, NeighborhoodDelegate neighborhood)
		{
			this.N = n;
			this.Neighborhood = neighborhood;

			this.Counts = new Dictionary<int?[,], int>(new SubimageComparer());
		}

		public void LoadFromImage(Bitmap image)
		{
            //int px = 0;

            BitmapData bmpData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb
                );
            int stride = bmpData.Stride;
            int dataSize = bmpData.Stride * bmpData.Height;
            
            byte[] bytes = new byte[dataSize];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, bytes, 0, dataSize);

            image.UnlockBits(bmpData);

			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
                    //if (px % 1000 == 0)
                    //{
                    //    Console.WriteLine(px);
                    //}

                    AddPixel(bytes, stride, image.Width, image.Height, new Point(x, y));

                    //px++;
				}
			}
		}

        private int?[,] ExtractNeighborhoodSubimage(byte[] imageData, int stride, int width, int height, Point center)
        {
            bool[,] neighbors = this.Neighborhood(this.N);

            // build subimage array to the specifications of the neighbors array
            int?[,] subimage = (int?[,])Array.CreateInstance(
                typeof(int?), 
                new int[] { neighbors.GetLength(0), neighbors.GetLength(1) },
                new int[] { neighbors.GetLowerBound(0), neighbors.GetLowerBound(1) }
                );

            //BitmapData bmpData = image.LockBits(
            //    new Rectangle(),
            //    ImageLockMode.ReadOnly,
            //    PixelFormat.Format24bppRgb
            //    );
            //int dataSize = bmpData.Stride * bmpData.Height;
            //byte[] bytes = new byte[dataSize];

            // now load in pixels from the image
            for (int x = neighbors.GetLowerBound(0); x <= neighbors.GetUpperBound(0); x++)
            {
                for (int y = neighbors.GetLowerBound(1); y <= neighbors.GetUpperBound(1); y++)
                {
                    int imageX = center.X + x, imageY = center.Y + y;

                    if (neighbors[x, y])
                    {
                        // pixel is in the neighborhood; actually load pixel data
                        if (imageX < 0 || imageX >= width || imageY < 0 || imageY >= height)
                        {
                            // pixel is out-of-bounds; use -1 as a "no pixel here" sentinel
                            subimage[x, y] = -1;
                        }
                        else
                        {
                            // pixel is in-bounds
                            int offsetR = stride * imageY + (imageX * 3);
                            int pixelValue = imageData[offsetR] << 16 | imageData[offsetR + 1] << 8 | imageData[offsetR + 2];

                            subimage[x, y] = pixelValue;
                        }
                    }
                    else
                    {
                        // pixel isn't in the neighborhood
                        subimage[x, y] = null;
                    }
                }
            }

            return subimage;
        }

        private class SubimageComparer : IEqualityComparer<int?[,]>
        {
            public bool Equals(int?[,] x, int?[,] y)
            {
                // short circuit if the two arrays are reference-equal
                if (object.ReferenceEquals(x, y)) return true;
                // no non-null array is equal to null
                if (x == null && y != null) return false;

                // no arrays with different bounds are equal
                if (x.GetLowerBound(0) != y.GetLowerBound(0) || 
                    x.GetLowerBound(1) != y.GetLowerBound(1) ||
                    x.GetUpperBound(0) != y.GetUpperBound(0) ||
                    x.GetUpperBound(1) != y.GetUpperBound(1))
                {
                    return false;
                }

                // compare elements
                for (int i = x.GetLowerBound(0); i <= x.GetUpperBound(0); i++)
                {
                    for (int j = x.GetLowerBound(1); j <= x.GetUpperBound(1); j++)
                    {
                        if (x[i, j] != y[i, j]) return false;
                    }
                }

                return true;
            }

            public int GetHashCode(int?[,] obj)
            {
                int hash = 0;

                hash += obj.GetLowerBound(0).GetHashCode();
                hash += obj.GetLowerBound(1).GetHashCode();
                hash += obj.GetUpperBound(0).GetHashCode();
                hash += obj.GetUpperBound(1).GetHashCode();

                // contents
                for (int i = obj.GetLowerBound(0); i <= obj.GetUpperBound(0); i++)
                {
                    for (int j = obj.GetLowerBound(1); j <= obj.GetUpperBound(1); j++)
                    {
                        hash += obj[i, j].GetHashCode();
                    }
                }

                return hash;
            }
        }

		private void AddPixel(byte[] imageData, int stride, int width, int height, Point pixel)
		{
            // extract neighborhood subimage
            int?[,] neighborhoodSubimage = ExtractNeighborhoodSubimage(imageData, stride, width, height, pixel);

            // increment model count
            int count = 0;            
            this.Counts.TryGetValue(neighborhoodSubimage, out count);

            count += 1;
            this.Counts[neighborhoodSubimage] = count;
		}

		private int N { get; set; }
		private NeighborhoodDelegate Neighborhood { get; set; }

		private Dictionary<int?[,], int> Counts { get; set; }
	}
}