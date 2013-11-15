using System;
using System.Drawing;

namespace JpegEBooks.ImageModel
{
	public class Model
	{
		public Model(int maxN, NeighborhoodDelegate neighborhood)
		{
            this.Submodels = new MarkovModel[maxN + 1];

            for (int i = 0; i < maxN; i++)
            {
                this.Submodels[i] = new MarkovModel(i, neighborhood);
            }
		}

        public void LoadImage(Bitmap image)
        {
            foreach (MarkovModel mm in this.Submodels)
            {
                mm.LoadFromImage(image);
            }
        }

        private MarkovModel[] Submodels { get; set; }
	}
}

