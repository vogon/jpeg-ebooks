using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

        private int TILE_SIZE = 1;

        private Color[,] ExtractTile(Bitmap image, int tileSize, int tileX, int tileY)
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

        private void BlitTile(Bitmap image, Color[,] tile, int tileSize, int tileX, int tileY)
        {
            for (int ypx = 0; ypx < tileSize; ypx++)
            {
                for (int xpx = 0; xpx < tileSize; xpx++)
                {
                    image.SetPixel(tileX * tileSize + xpx, tileY * tileSize + ypx, tile[xpx, ypx]);
                }
            }
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

        private class TileGenerationRecord
        {
            public TileGenerationRecord(Point point, bool isGenerated)
            {
                this.Point = point;
                this.IsGenerated = isGenerated;
            }

            public Point Point { get; set; }
            public bool IsGenerated { get; set; }
        }

        private Digraph<TileGenerationRecord> MakeDependencyGraph(int w, int h, int tileSize)
        {
            Digraph<TileGenerationRecord> depGraph = new Digraph<TileGenerationRecord>();

            int wTile = w / tileSize, hTile = h / tileSize;
            Digraph<TileGenerationRecord>.Vertex[,] vertices = new Digraph<TileGenerationRecord>.Vertex[wTile, hTile];

            // build vertices
            for (int y = 0; y < hTile; y++)
            {
                for (int x = 0; x < wTile; x++)
                {
                    vertices[x, y] = new Digraph<TileGenerationRecord>.Vertex(new TileGenerationRecord(new Point(x, y), false));
                    depGraph.Add(vertices[x, y]);
                }
            }

            // build edges
            for (int y = 0; y < hTile; y++)
            {
                for (int x = 0; x < wTile; x++)
                {
                    if (x != 0) depGraph.DrawEdge(vertices[x - 1, y], vertices[x, y]);
                    if (y != 0) depGraph.DrawEdge(vertices[x, y - 1], vertices[x, y]);
                }
            }

            return depGraph;
        }

        public void LoadImage(Bitmap image)
        {
            // build dep graph for loading image
            Digraph<TileGenerationRecord> depGraph = MakeDependencyGraph(image.Width, image.Height, TILE_SIZE);

            foreach (Digraph<TileGenerationRecord>.Vertex v in depGraph.Vertices)
            {
                List<double[]> positions =
                    GetPredecessorModelPositions(image, TILE_SIZE, v, new double[] { }, this.maxN, 1, false, false);
                Color[,] thisSubimage = ExtractTile(image, TILE_SIZE, v.Label.Point.X, v.Label.Point.Y);

                foreach (double[] pos in positions)
                {
                    if (pos.Length == 0)
                    {
                        this.nullaryModel.Add(thisSubimage);
                    }
                    else
                    {
                        int n = pos.Length / 3;
                        SuccessorTileModel model = this.naryModels[n - 1].Get(pos);

                        if (model == null)
                        {
                            model = new SuccessorTileModel();
                            model.Add(thisSubimage);
                            this.naryModels[n - 1].Insert(pos, model);
                        }
                        else
                        {
                            model.Add(thisSubimage);
                        }
                    }
                }
            }
        }
        
        private List<double[]> GetPredecessorModelPositions(
            Bitmap image, int tileSize, Digraph<TileGenerationRecord>.Vertex v, double[] partialPos, 
            int nLeft, int nSkip, bool suppressSubpaths, bool suppressUngenerated)
        {
            List<double[]> positions = new List<double[]>();

            if (nSkip != 0)
            {
                // skip this node and go to its predecessors
                if (v.Predecessors.Length == 0)
                {
                    positions.Add(partialPos);
                }

                foreach (Digraph<TileGenerationRecord>.Vertex pred in v.Predecessors)
                {
                    if (pred.Label.IsGenerated || !suppressUngenerated)
                    {
                        positions.AddRange(GetPredecessorModelPositions(
                            image, tileSize, pred, partialPos, nLeft, nSkip - 1, suppressSubpaths, suppressUngenerated
                            ));
                    }
                    else
                    {
                        Console.WriteLine("hit un-generated tile at {0}, {1}", pred.Label.Point.X, pred.Label.Point.Y);
                    }
                }
            }
            else if (nLeft != 0)
            {
                // add this node's position to the partial and go to its predecessors
                if (!suppressSubpaths || v.Predecessors.Length == 0)
                {
                    positions.Add(partialPos);
                }

                double[] thisPosition = AverageColor(image, tileSize, v.Label.Point.X, v.Label.Point.Y);
                double[] nextPartial = new double[partialPos.Length + thisPosition.Length];

                Array.Copy(partialPos, nextPartial, partialPos.Length);
                Array.Copy(thisPosition, 0, nextPartial, partialPos.Length, thisPosition.Length);

                foreach (Digraph<TileGenerationRecord>.Vertex pred in v.Predecessors)
                {
                    if (pred.Label.IsGenerated || !suppressUngenerated)
                    {
                        positions.AddRange(GetPredecessorModelPositions(
                            image, tileSize, pred, nextPartial, nLeft - 1, nSkip, suppressSubpaths, suppressUngenerated
                            ));
                    }
                    else
                    {
                        Console.WriteLine("hit un-generated tile at {0}, {1}", pred.Label.Point.X, pred.Label.Point.Y);
                    }
                }
            }
            else
            {
                // done
                positions.Add(partialPos);
            }

            return positions;
        }

        public Bitmap GenImage(int wTiles, int hTiles)
        {
            int w = wTiles * TILE_SIZE, h = hTiles * TILE_SIZE;
            Bitmap img = new Bitmap(w, h);
            Random rng = new Random();
            
            Digraph<TileGenerationRecord> depGraph = MakeDependencyGraph(w, h, TILE_SIZE);

            while (true)
            {
                IEnumerable<Digraph<TileGenerationRecord>.Vertex> tilesToGenerate = 
                    depGraph.Vertices
                        .Where(v => (v.Label.IsGenerated == false))
                        .OrderBy(v => v.Predecessors.Count(vv => !vv.Label.IsGenerated));

                if (tilesToGenerate.Count() == 0)
                {
                    break;
                }
                
                Digraph<TileGenerationRecord>.Vertex nextTileToGenerate = tilesToGenerate.First();

                Console.WriteLine("{0}, {1}", nextTileToGenerate.Label.Point.X, nextTileToGenerate.Label.Point.Y);

                // actually generate the tile.
                SuccessorTileModel mergedModel = new SuccessorTileModel();

                // grab a list of all of the model positions represented by all the predecessors to this tile
                List<double[]> positions = 
                    GetPredecessorModelPositions(img, TILE_SIZE, nextTileToGenerate, new double[] { }, this.maxN, 1, true, true);

                // look 'em all up
                foreach (double[] pos in positions)
                {
                    if (pos.Length == 0)
                    {
                        mergedModel.AddAll(this.nullaryModel);
                    }
                    else
                    {
                        int n = pos.Length / 3;
                        SuccessorTileModel[] models = this.naryModels[n - 1].RangeSearch(pos, 2);

                        foreach (SuccessorTileModel model in models)
                        {
                            mergedModel.AddAll(model);
                        }
                    }
                }

                // choose randomly from that model
                Color[,] tile = mergedModel.ChooseRandomly(rng);

                if (tile != null)
                {
                    BlitTile(img, tile, TILE_SIZE, nextTileToGenerate.Label.Point.X, nextTileToGenerate.Label.Point.Y);
                }

                nextTileToGenerate.Label.IsGenerated = true;
            }

            return img;
        }

        private readonly int maxN;
        private SuccessorTileModel nullaryModel;
        private KDMap<SuccessorTileModel>[] naryModels;
    }
}

