using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JpegEBooks.ImageModel
{
    class Digraph<VertexLabel>
    {
        public Digraph()
        {
            this.Vertices = new HashSet<Vertex>();
        }

        public void Add(Vertex v)
        {
            this.Vertices.Add(v);
        }

        public bool Contains(Vertex v)
        {
            return this.Vertices.Contains(v);
        }

        public void DrawEdge(Vertex from, Vertex to)
        {
            Debug.Assert(this.Contains(from) && this.Contains(to));
            Vertex.DrawEdge(from, to);
        }

        public void EraseEdge(Vertex from, Vertex to)
        {
            Debug.Assert(this.Contains(from) && this.Contains(to));
            Vertex.EraseEdge(from, to);
        }

        public ISet<Vertex> Vertices { get; private set; }

        public class Vertex
        {
            public Vertex(VertexLabel label) 
            {
                this.Label = label;
                this.InEdges = new HashSet<Vertex>();
                this.OutEdges = new HashSet<Vertex>();
            }

            public Vertex[] Predecessors
            {
                get
                {
                    return InEdges.ToArray();
                }
            }

            public int InDegree
            {
                get
                {
                    return InEdges.Count;
                }
            }

            public Vertex[] Successors
            {
                get
                {
                    return OutEdges.ToArray();
                }
            }

            public int OutDegree
            {
                get
                {
                    return OutEdges.Count;
                }
            }

            public static void DrawEdge(Vertex from, Vertex to)
            {
                to.InEdges.Add(from);
                from.OutEdges.Add(to);
            }

            public static void EraseEdge(Vertex from, Vertex to)
            {
                to.InEdges.Remove(from);
                from.OutEdges.Remove(to);
            }

            public VertexLabel Label { get; private set; }

            private ISet<Vertex> InEdges { get; set; }
            private ISet<Vertex> OutEdges { get; set; }
        }
    }
}
