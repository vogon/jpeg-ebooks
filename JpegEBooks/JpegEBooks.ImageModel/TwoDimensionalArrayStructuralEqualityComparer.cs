using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JpegEBooks.ImageModel
{
    class TwoDimensionalArrayStructuralEqualityComparer<T> : IEqualityComparer<T[,]>
    {
        static TwoDimensionalArrayStructuralEqualityComparer()
        {
            Instance = new TwoDimensionalArrayStructuralEqualityComparer<T>();
        }

        public static TwoDimensionalArrayStructuralEqualityComparer<T> Instance { get; private set; }

        private TwoDimensionalArrayStructuralEqualityComparer() { }

        public bool Equals(T[,] x, T[,] y)
        {
            if (x.GetLowerBound(0) != y.GetLowerBound(0)) return false;
            if (x.GetUpperBound(0) != y.GetUpperBound(0)) return false;
            if (x.GetLowerBound(1) != y.GetLowerBound(1)) return false;
            if (x.GetUpperBound(1) != y.GetUpperBound(1)) return false;

            for (int j = x.GetLowerBound(1); j <= x.GetUpperBound(1); j++)
            {
                for (int i = x.GetLowerBound(0); i <= x.GetUpperBound(0); i++)
                {
                    if (!x[i, j].Equals(y[i, j])) return false;
                }
            }

            return true;
        }

        public int GetHashCode(T[,] obj)
        {
            int hash = 0;

            hash += obj.GetLowerBound(0).GetHashCode();
            hash += obj.GetUpperBound(0).GetHashCode();
            hash += obj.GetLowerBound(1).GetHashCode();
            hash += obj.GetUpperBound(1).GetHashCode();

            for (int j = obj.GetLowerBound(1); j <= obj.GetUpperBound(1); j++)
            {
                for (int i = obj.GetLowerBound(0); i <= obj.GetUpperBound(0); i++)
                {
                    hash += obj[i, j].GetHashCode();
                }
            }

            return hash;
        }
    }
}
