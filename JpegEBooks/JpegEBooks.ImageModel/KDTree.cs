using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JpegEBooks.ImageModel
{
    class KDMap<V>
    {
        public KDMap(int k)
        {
            this.k = k;
        }

        public void Insert(double[] position, V value)
        {
            if (position.Length != this.k)
            {
                throw new ArgumentException("position has wrong dimensionality");
            }

            if (root == null)
            {
                // 0 nodes in tree; create a node and use it as root
                Node newRoot = new Node(position, value);
                root = newRoot;
            }
            else
            {
                this.root.Insert(0, this.k, position, value);
            }
        }

        public V Get(double[] position)
        {
            if (this.root == null)
            {
                return default(V);
            }
            else
            {
                Node node = this.root.Get(0, this.k, position);

                if (node == null)
                {
                    return default(V);
                }
                else
                {
                    return node.Value;
                }
            }
        }

        public V[] RangeSearch(double[] position, double radius)
        {
            if (this.root == null)
            {
                return new V[] { };
            }
            else
            {
                return this.root.RangeSearch(0, this.k, position, radius).Select((node) => node.Value).ToArray();
            }
        }

        public int Count()
        {
            return (this.root != null) ? this.root.Count() : 0;
        }

        private class Node
        {
            public Node(double[] position, V value)
            {
                this.Position = position;
                this.Value = value;
            }

            public readonly double[] Position;

            public Node Left;
            public Node Right;

            public readonly V Value;

            public Node Insert(int depth, int k, double[] position, V value)
            {
                // check to make sure this node isn't already at the new node's position
                if (StructuralComparisons.StructuralEqualityComparer.Equals(position, this.Position))
                {
                    throw new ArgumentException("position is already occupied");
                }

                // figure out whether position is before or after this node
                int dimension = depth % k;
                double rootLocation = this.Position[dimension];
                double location = position[dimension];

                // add new node to the subtree rooted here
                if (location.CompareTo(rootLocation) < 0)
                {
                    // child is < this node; add to left subtree
                    if (this.Left == null)
                    {
                        // left subtree is empty; build new node
                        Node newNode = new Node(position, value);

                        this.Left = newNode;
                        return newNode;
                    }
                    else
                    {
                        // recurse
                        return this.Left.Insert(depth + 1, k, position, value);
                    }
                }
                else
                {
                    // child is > this node; add to right subtree
                    if (this.Right == null)
                    {
                        // right subtree is empty; build new node
                        Node newNode = new Node(position, value);

                        this.Right = newNode;
                        return newNode;
                    }
                    else
                    {
                        // recurse
                        return this.Right.Insert(depth + 1, k, position, value);
                    }
                }
            }

            public Node Get(int depth, int k, double[] position)
            {
                // check to see if we're already there
                if (StructuralComparisons.StructuralEqualityComparer.Equals(position, this.Position))
                {
                    return this;
                }

                // figure out whether position is before or after this node
                int dimension = depth % k;
                double rootLocation = this.Position[dimension];
                double location = position[dimension];

                // search within child subtrees
                if (location.CompareTo(rootLocation) < 0)
                {
                    // child is < this node; search left subtree
                    if (this.Left == null)
                    {
                        // left subtree is empty; search unsuccessful
                        return null;
                    }
                    else
                    {
                        // recurse
                        return this.Left.Get(depth + 1, k, position);
                    }
                }
                else
                {
                    // child is > this node; search right subtree
                    if (this.Right == null)
                    {
                        // right subtree is empty; search unsuccessful
                        return null;
                    }
                    else
                    {
                        // recurse
                        return this.Right.Get(depth + 1, k, position);
                    }
                }
            }

            public IList<Node> RangeSearch(int depth, int k, double[] position, double radius)
            {
                //StringBuilder sb = new StringBuilder();
                //sb.Append("[");

                //for (int i = 0; i < k; i++) {
                //    sb.Append(position[i]);
                //    sb.Append(", ");
                //}

                //sb.Append("]");

                //Console.WriteLine("RangeSearch({0}, {1}, {2}, {3})", depth, k, sb.ToString(), radius);

                // check whether the root is within radius of position
                List<Node> values = new List<Node>();
                double dist = 0;

                for (int i = 0; i < k; i++)
                {
                    dist += Math.Pow((position[i] - this.Position[i]), 2);
                }

                if (dist <= radius * radius)
                {
                    values.Add(this);
                }

                // recurse to subtrees
                // "before" subtree: if position - radius is after the root, prune
                // "after" subtree: if position + radius is before the root, prune
                int dimension = depth % k;
                double location = this.Position[dimension];
                double searchLocation = position[dimension];

                if (searchLocation - radius <= location)
                {
                    if (this.Left != null)
                    {
                        values.AddRange(this.Left.RangeSearch(depth + 1, k, position, radius));
                    }
                }
                else
                {
                    //Console.WriteLine("pruned left subtree ({0} + {1} < {2})", searchLocation, radius, location);
                }

                if (searchLocation + radius >= location)
                {
                    if (this.Right != null)
                    {
                        values.AddRange(this.Right.RangeSearch(depth + 1, k, position, radius));
                    }
                }
                else
                {
                    //Console.WriteLine("pruned right subtree ({0} - {1} > {2})", searchLocation, radius, location);
                }

                return values;
            }
            
            public int Count()
            {
                int accum = 1;

                if (this.Left != null)
                {
                    accum += this.Left.Count();
                }

                if (this.Right != null)
                {
                    accum += this.Right.Count();
                }

                return accum;
            }
        }


        private readonly int k;
        private Node root;
    }
}
