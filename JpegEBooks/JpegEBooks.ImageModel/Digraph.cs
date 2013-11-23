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
            
        }

        private HashSet<Vertex> Vertices { get; set; }

        public class Vertex
        {
            public Vertex(VertexLabel label) 
            {
                this.Label = Label;
            }

            public Vertex[] Predecessors
            {
                get
                {
                    return InEdges.ToArray();
                }
            }

            public Vertex[] Successors
            {
                get
                {
                    return OutEdges.ToArray();
                }
            }

            public VertexLabel Label { get; private set; }

            private IList<Vertex> InEdges { get; set; }
            private IList<Vertex> OutEdges { get; set; }
        }
    }
}
