using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

namespace JpegEBooks.ImageModel
{
    public class Model
    {
        public Model(int maxN)
        {
            this.nullaryModel = new SuccessorTileModel();
            this.naryModels = new KDMap<int, SuccessorTileModel>[maxN];
        }

        private class SuccessorTileModel
        {
            public SuccessorTileModel()
            {
                this.counts = new Dictionary<int[,], int>(TwoDIntArrayStructuralEqualityComparer.Instance);
                this.grandTotal = 0;
            }

            public void Add(int[,] tile) 
            {
                if (this.counts.ContainsKey(tile))
                {
                    this.counts[tile] += 1;
                }
                else
                {
                    this.counts[tile] = 1;
                }
            }

            private Dictionary<int[,], int> counts;
            private int grandTotal;
        }

        private class TwoDIntArrayStructuralEqualityComparer : IEqualityComparer<int[,]>
        {
            static TwoDIntArrayStructuralEqualityComparer()
            {
                Instance = new TwoDIntArrayStructuralEqualityComparer();
            }

            public static TwoDIntArrayStructuralEqualityComparer Instance { get; private set; }

            private TwoDIntArrayStructuralEqualityComparer() { }

            public bool Equals(int[,] x, int[,] y)
            {
                return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
            }

            public int GetHashCode(int[,] obj)
            {
                return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
            }
        }

        public void LoadImage(Bitmap image)
        {
        }

        private SuccessorTileModel nullaryModel;
        private KDMap<int, SuccessorTileModel>[] naryModels;
    }
}

