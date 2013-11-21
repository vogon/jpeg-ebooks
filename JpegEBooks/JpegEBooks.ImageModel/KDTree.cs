using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JpegEBooks.ImageModel
{
    class KDMap<Coord, V> where Coord : IComparable<Coord>, IEquatable<Coord>
    {
        public KDMap(int k)
        {
            this.k = k;
        }

        public void Insert(Coord[] position, V value)
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

        public V Get(Coord[] position)
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

        private class Node
        {
            public Node(Coord[] position, V value)
            {
                this.Position = position;
                this.Value = value;
            }

            public readonly Coord[] Position;

            public Node Left;
            public Node Right;

            public readonly V Value;

            public Node Insert(int depth, int k, Coord[] position, V value)
            {
                // check to make sure this node isn't already at the new node's position
                if (StructuralComparisons.StructuralEqualityComparer.Equals(position, this.Position))
                {
                    throw new ArgumentException("position is already occupied");
                }

                // figure out whether position is before or after this node
                int dimension = depth % k;
                Coord rootLocation = this.Position[dimension];
                Coord location = position[dimension];

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

            public Node Get(int depth, int k, Coord[] position)
            {
                // check to see if we're already there
                if (StructuralComparisons.StructuralEqualityComparer.Equals(position, this.Position))
                {
                    return this;
                }

                // figure out whether position is before or after this node
                int dimension = depth % k;
                Coord rootLocation = this.Position[dimension];
                Coord location = position[dimension];

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
        }

        private readonly int k;
        private Node root;
    }
}
