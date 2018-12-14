using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apriori
{
    class Node
    {
        public String Item;
        public List<Node>   NextItem;
        public Node PreviousItem, NextSame;
        public int weight;
        public Node()
        {
            PreviousItem = null;
            NextItem = new List<Node>();
            NextSame = null;
            weight = 0;
            Item = "root";
        }
    }
}
