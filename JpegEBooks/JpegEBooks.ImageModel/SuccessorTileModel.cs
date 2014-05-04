using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace JpegEBooks.ImageModel
{
    [Serializable]
    class SuccessorTileModel
    {
        public SuccessorTileModel()
        {
            this.counts = new Dictionary<Color, double>();
            this.grandTotal = 0;
        }

        public void Add(Color tile)
        {
            this.AddWeight(tile, 1.0);
        }

        private void AddWeight(Color tile, double weight)
        {
            if (this.counts.ContainsKey(tile))
            {
                this.counts[tile] += weight;
            }
            else
            {
                this.counts[tile] = weight;
            }

            this.grandTotal += weight;
        }

        /// <summary>
        ///     Add every element from the other model, with weights multiplied by the rate
        /// </summary>
        /// <param name="model">the model whose elements should be added</param>
        /// <param name="rate">the rate to weight model's elements by, with 1.0 the default</param>
        public void AddAll(SuccessorTileModel model, double rate = 1.0)
        {
            foreach (KeyValuePair<Color, double> kvp in model.counts)
            {
                this.AddWeight(kvp.Key, kvp.Value * rate);
            }
        }

        public Color ChooseRandomly(Random rng)
        {
            double rand = rng.NextDouble() * grandTotal;

            foreach (KeyValuePair<Color, double> tile in this.counts)
            {
                if (rand < tile.Value)
                {
                    return tile.Key;
                }
                else
                {
                    rand -= tile.Value;
                }
            }

            return Color.Magenta;
        }

        private Dictionary<Color, double> counts;
        private double grandTotal;
    }
}
