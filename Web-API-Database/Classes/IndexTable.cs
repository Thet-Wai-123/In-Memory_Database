using CodeExMachina;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Xml.Linq;

namespace Web_API_Database.Classes
{
    public abstract class IndexTable
    {
        public abstract List<DataRow> Search(object keyValue, string op);
        public abstract void Insert(int columnIndex, DataRow row);
        public abstract void Delete(int columnIndex, DataRow row);

        protected int degree = 2;
    }

    public class IndexTable<T> :IndexTable
        where T : IComparable<T>
    {
        private readonly BTree<Node<T>> btree;

        public IndexTable(int columnIndex, List<DataRow> rows)
        {
            btree = new(degree, new KeyComparer<T>());
            foreach (DataRow row in rows)
            {
                Node<T> newNode = new(row[columnIndex], [row]);
                HandleInsertWithPossibleDuplicate(newNode);
            }
        }


        public override List<DataRow> Search(object keyValue, string op)
        {
            List<DataRow> matchingRows = [];
            T key = (T)keyValue;
            Node<T> pivotNode = new(key, null);
            List<Node<T>> foundNodes = [];
            if (op == "==")
            {
                var result = (btree.Get(pivotNode));
                if (result != null)
                {
                    foundNodes.Add(result);
                }
            }
            else if (op == ">=")
            {
                btree.AscendGreaterOrEqual(pivotNode, (Node<T> n) =>
                {
                    foundNodes.Add(n);
                    return true;
                });
            }
            else if (op == "<=")
            {
                btree.DescendLessOrEqual(pivotNode, (Node<T> n) =>
                {
                    foundNodes.Add(n);
                    return true;
                });
            }
            else
            {
                throw new ArgumentException($"Unsupported operator: {op}");
            }
            foreach (Node<T> foundNode in foundNodes)
            {
                matchingRows.AddRange(foundNode.Value);
            }
            return matchingRows;
        }

        public override void Insert(int columnIndex, DataRow row)
        {
            Node<T> newNode = new(row[columnIndex], [row]);
            HandleInsertWithPossibleDuplicate(newNode);
        }

        public override void Delete(int columnIndex, DataRow row)
        {
            btree.Delete(new Node<T>(row[columnIndex], [row]));
        }

        private class Node<TKey>
            where TKey : IComparable<TKey>
        {
            public TKey Key
            {
                get;
            }
            public List<DataRow> Value
            {
                get;
            }

            public Node(TKey key, List<DataRow> value)
            {
                Key = key;
                Value = value;
            }
        }

        private class KeyComparer<TKey> :Comparer<Node<TKey>>
            where TKey : IComparable<TKey>
        {
            public override int Compare(Node<TKey> a, Node<TKey> b)
            {
                return a.Key.CompareTo(b.Key);
            }
        }

        private void HandleInsertWithPossibleDuplicate(Node<T> node)
        {
            //If the key already exists, we need to combine the values
            var prevNode = btree.ReplaceOrInsert(node);
            if (prevNode != null)
            {
                node.Value.AddRange(prevNode.Value);
            }
        }
    }
}
