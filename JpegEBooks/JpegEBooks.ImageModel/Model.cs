using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace JpegEBooks.ImageModel
{
    [Serializable]
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

        private Digraph<TileGenerationRecord> MakeDependencyGraph(int w, int h)
        {
            Digraph<TileGenerationRecord> depGraph = new Digraph<TileGenerationRecord>();

            int wTile = w, hTile = h;
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
            Digraph<TileGenerationRecord> depGraph = MakeDependencyGraph(image.Width, image.Height);
            int nPixelsConsumed = 0;

            foreach (Digraph<TileGenerationRecord>.Vertex v in depGraph.Vertices)
            {
                List<byte[]> positions =
                    GetPredecessorModelPositions(image, 1, v, new byte[] { }, this.maxN, 1, false, false);
                Color thisPixel = image.GetPixel(v.Label.Point.X, v.Label.Point.Y);

                foreach (byte[] pos in positions)
                {
                    if (pos.Length == 0)
                    {
                        this.nullaryModel.Add(thisPixel);
                    }
                    else
                    {
                        int n = pos.Length / 3;
                        SuccessorTileModel model = this.naryModels[n - 1].Get(pos);

                        if (model == null)
                        {
                            model = new SuccessorTileModel();
                            model.Add(thisPixel);
                            this.naryModels[n - 1].Insert(pos, model);
                        }
                        else
                        {
                            model.Add(thisPixel);
                        }
                    }
                }

                nPixelsConsumed++;

                if (nPixelsConsumed % 10000 == 0)
                {
                    Console.Write(".");
                    Console.Out.Flush();
                }
            }

            Console.WriteLine();
        }
        
        private List<byte[]> GetPredecessorModelPositions(
            Bitmap image, int tileSize, Digraph<TileGenerationRecord>.Vertex v, byte[] partialPos, 
            int nLeft, int nSkip, bool suppressSubpaths, bool suppressUngenerated)
        {
            List<byte[]> positions = new List<byte[]>();

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

                Color c = image.GetPixel(v.Label.Point.X, v.Label.Point.Y);
                byte[] thisPosition = new byte[] { c.R, c.G, c.B };
                byte[] nextPartial = new byte[partialPos.Length + thisPosition.Length];

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
            int w = wTiles, h = hTiles;
            Bitmap img = new Bitmap(w, h);
            Random rng = new Random();
            
            Digraph<TileGenerationRecord> depGraph = MakeDependencyGraph(w, h);

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
                List<byte[]> positions = 
                    GetPredecessorModelPositions(img, 1, nextTileToGenerate, new byte[] { }, this.maxN, 1, true, true);

                // look 'em all up
                foreach (byte[] pos in positions)
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
                Color c = mergedModel.ChooseRandomly(rng);

                if (c != null)
                {
                    img.SetPixel(nextTileToGenerate.Label.Point.X, nextTileToGenerate.Label.Point.Y, c);
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

