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
            this.maxN = maxN;
            this.nullaryModel = new SuccessorTileModel();
            this.naryModels = new KDMap<SuccessorTileModel>[maxN];

            for (int i = 1; i <= maxN; i++)
            {
                this.naryModels[i - 1] = new KDMap<SuccessorTileModel>(3 * i);
            }
        }

        private class SuccessorTileModel
        {
            public SuccessorTileModel()
            {
                this.counts = new Dictionary<Color[,], double>(TwoDimensionalArrayStructuralEqualityComparer<Color>.Instance);
                this.grandTotal = 0;
            }

            public void Add(Color[,] tile) 
            {
                this.AddWeight(tile, 1.0);
            }

            private void AddWeight(Color[,] tile, double weight)
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
                foreach (KeyValuePair<Color[,], double> kvp in model.counts)
                {
                    this.AddWeight(kvp.Key, kvp.Value * rate);
                }
            }

            public Color[,] ChooseRandomly(Random rng)
            {
                double rand = rng.NextDouble() * grandTotal;

                foreach (KeyValuePair<Color[,], double> tile in this.counts)
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

                return null;
            }

            private Dictionary<Color[,], double> counts;
            private double grandTotal;
        }

        private int TILE_SIZE = 4;

        private Color[,] ExtractSubimage(Bitmap image, int tileSize, int tileX, int tileY)
        {
            Color[,] subimage = new Color[tileSize, tileSize];

            for (int ypx = 0; ypx < tileSize; ypx++)
            {
                for (int xpx = 0; xpx < tileSize; xpx++)
                {
                    subimage[xpx, ypx] = image.GetPixel(tileX * tileSize + xpx, tileY * tileSize + ypx);
                }
            }

            return subimage;
        }

        private double[] AverageColor(Bitmap image, int tileSize, int tileX, int tileY)
        {
            double[] components = new double[3];
            int n = 0;

            for (int ypx = 0; ypx < tileSize; ypx++)
            {
                for (int xpx = 0; xpx < tileSize; xpx++)
                {
                    Color c = image.GetPixel(tileX * tileSize + xpx, tileY * tileSize + ypx);

                    components[0] += c.R;
                    components[1] += c.G;
                    components[2] += c.B;
                    n++;
                }
            }

            components[0] /= n;
            components[1] /= n;
            components[2] /= n;

            return components;
        }

        private double[] ModelPosition(Bitmap image, int xTile, int yTile, int tileSize, int modelArity)
        {
            double[] pos = new double[modelArity * 3];

            for (int i = 1; i <= modelArity; i++)
            {
                double[] avgColor = AverageColor(image, TILE_SIZE, xTile - i, yTile);

                Array.Copy(avgColor, 0, pos, (i - 1) * 3, 3);
            }

            return pos;
        }

        public void LoadImage(Bitmap image)
        {
            // ersatz implementation for right now:
            // - tiles depend on tiles to the left of them only
            // - no inter-row dependency
            for (int y = 0; (y + 1) * TILE_SIZE <= image.Height; y++)
            {
                for (int x = 0; (x + 1) * TILE_SIZE <= image.Width; x++)
                {
                    // extract this tile
                    Color[,] thisSubimage = ExtractSubimage(image, TILE_SIZE, x, y);
            
                    // figure out how many tiles of color data are actually available
                    int actualArity = Math.Min(x, this.maxN);
                    double[] pos = ModelPosition(image, x, y, TILE_SIZE, actualArity);

                    // for the 0-ary, 1-ary, ... maxN-ary models:
                    for (int n = 0; n < actualArity; n++)
                    {
                        if (n == 0)
                        {
                            this.nullaryModel.Add(thisSubimage);
                        }
                        else
                        {
                            // the i'th n-ary model is the (i + 1)-ary model, since the nullary model is separate
                            int modelIdx = n - 1;

                            double[] partialPos = new double[n * 3];
                            Array.Copy(pos, partialPos, n * 3);

                            SuccessorTileModel model = this.naryModels[modelIdx].Get(partialPos);

                            if (model == null)
                            {
                                model = new SuccessorTileModel();
                                model.Add(thisSubimage);
                                this.naryModels[modelIdx].Insert(partialPos, model);
                            }
                            else
                            {
                                model.Add(thisSubimage);
                            }
                        }
                    }
                }
            }

            // build dep graph for loading image

            // for each tile t:
            //     load tile into models:
            //         traverse dep graph depth-first with depth limit maxN:
            //             visit each predecessor of t, storing path
            //             for n = 0 upto path length:
            //                 pos = []
            //                 for each predecessor on the first n steps of path:
            //                     r, g, b = extract color from tile
            //                     pos << r, g, b
            //                 add t to position pos in the n-ary model
        }

        public Bitmap GenImage(int wTiles, int hTiles)
        {
            Bitmap img = new Bitmap(wTiles * TILE_SIZE, hTiles * TILE_SIZE);
            Random rng = new Random();

            // ersatz implementation
            for (int yTile = 0; yTile < hTiles; yTile++)
            {
                // gen left edge based on 0-ary model
                Color[,] tile = this.nullaryModel.ChooseRandomly(rng);

                for (int ypx = 0; ypx < TILE_SIZE; ypx++)
                {
                    for (int xpx = 0; xpx < TILE_SIZE; xpx++)
                    {
                        img.SetPixel(xpx, yTile * TILE_SIZE + ypx, tile[xpx, ypx]);
                    }
                }

                // gen everything else based on min(x, maxN)-ary model
                for (int xTile = 1; xTile < wTiles; xTile++)
                {
                    Console.Write("{0}, {1}: ", xTile, yTile);

                    int actualMaxArity = Math.Min(xTile, this.maxN);

                    for (int modelArity = actualMaxArity; modelArity >= 1; modelArity--)
                    {
                        double[] pos = ModelPosition(img, xTile, yTile, TILE_SIZE, modelArity);
                        SuccessorTileModel[] submodels = this.naryModels[modelArity - 1].RangeSearch(pos, 10);
                        
                        if (submodels.Length > 0)
                        {
                            // we found a submodel to choose from; generate
                            SuccessorTileModel mergedModel = new SuccessorTileModel();

                            foreach (SuccessorTileModel model in submodels)
                            {
                                mergedModel.AddAll(model, 1.0);
                            }

                            tile = mergedModel.ChooseRandomly(rng);

                            for (int ypx = 0; ypx < TILE_SIZE; ypx++)
                            {
                                for (int xpx = 0; xpx < TILE_SIZE; xpx++)
                                {
                                    img.SetPixel(xTile * TILE_SIZE + xpx, yTile * TILE_SIZE + ypx, tile[xpx, ypx]);
                                }
                            }

                            Console.Write("! ({0})", submodels.Length);
                        }
                        else
                        {
                            // no submodel found; try a lower arity
                            Console.Write("?");
                            continue;
                        }
                    }

                    Console.WriteLine();
                }
            }

            // build dep graph for generating image

            // toGenerate = min-heap of nodes in dep graph ordered by number of predecessors
            // while toGenerate not empty:
            //     next = head(toGenerate)
            //     [TODO: code to JIT-generate any ungenerated predecessors goes here]
            //     traverse dep graph depth-first with depth limit maxN:
            //         [flergh, rest goes here later]

            return img;
        }

        private readonly int maxN;
        private SuccessorTileModel nullaryModel;
        private KDMap<SuccessorTileModel>[] naryModels;
    }
}

