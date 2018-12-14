using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows;
namespace Apriori
{
    class Program
    {
        //---------------------- Apriori BEGING ----------------

        static Dictionary<string, int> getC1(string[] Transactions)
        {
            Dictionary<string, int> ItemsSet = new Dictionary<string, int>();

            foreach (string TR in Transactions)
            {
                foreach (string Item in TR.Trim().Split(' '))
                {


                    if (ItemsSet.ContainsKey(Item))
                        ItemsSet[Item] += 1;
                    else
                        ItemsSet.Add(Item, 1);
                }
            }
            return ItemsSet;
        }
        static Dictionary<string, int> getCN(Dictionary<string, int> Ln_1, int N)
        {
            Dictionary<string, int> CN = new Dictionary<string, int>();
            //CONCATENATION
            for (int i = 0; i < Ln_1.Count; i++)
                for (int j = i + 1; j < Ln_1.Count; j++)
                {   //ELIMINATE OCCURENCE IN EACH TRANSACTION
                    String newItem = EliminateOcc(Ln_1.ElementAt(i).Key + "," + Ln_1.ElementAt(j).Key);
                    if (newItem.Split(',').Count() != N)
                        continue;

                    // ELIMINATE OCCURENCE IN TRANSACTION
                    if (!ItemExist(CN, newItem))
                    { //Console.WriteLine(newItem); 
                        CN.Add(newItem, 0);
                    }

                }
            Dictionary<string, int> _TempCount = new Dictionary<string, int>();

            //GET SUPPORT COUNT
            foreach (KeyValuePair<String, int> Tran in CN)
            {

                foreach (String Line in Transactions)
                {
                    string LineItem = Line.Trim();
                    if (LineItem.Split(' ').Count() < Tran.Key.Split(',').Count())
                        continue;
                    int cptLine = 0;
                    //COMPARE BETWEEN COMIBINITION AND LINE TRANSACTION
                    foreach (String ItemC in Tran.Key.Split(','))
                    {
                        foreach (String ItemL in Line.Trim().Split(' '))
                        {
                            if (ItemC.Equals(ItemL)) { cptLine++; continue; }
                        }

                    }
                    if (cptLine == Tran.Key.Split(',').Count())
                        if (_TempCount.ContainsKey(Tran.Key))
                            _TempCount[Tran.Key] += 1;
                        else
                            _TempCount.Add(Tran.Key, 1);

                }
            }
            foreach (KeyValuePair<String, int> Tran in _TempCount)
                CN[Tran.Key] = Tran.Value;

            return CN;
        }
        static Dictionary<string, int> getLN(Dictionary<string, int> C1)
        {
            Dictionary<string, int> L1 = new Dictionary<string, int>();

            for (int i = 0; i < C1.Count; i++)
            {
                if (C1.ElementAt(i).Value >= min_sup)
                    L1.Add(C1.ElementAt(i).Key, C1.ElementAt(i).Value);
            }
            return L1;
        }
        static Boolean ItemExist(Dictionary<string, int> C, String newItem)
        {

            foreach (KeyValuePair<string, int> Tran in C)
            {
                int cpt = 0;
                foreach (String ItemC in Tran.Key.Split(','))
                {
                    foreach (String ne in newItem.Trim().Split(','))
                    {
                        if (ItemC.Equals(ne)) { cpt++; continue; }
                    }

                }
                if (cpt == Tran.Key.Split(',').Count())
                    return true;
            }



            return false;
        }
        static float getAverage(Dictionary<string, int> Set)
        {
            int cpt = 0;
            foreach (KeyValuePair<string, int> Item in Set)
            {
                cpt += Item.Value;
            }
            return cpt / Set.Count();
        }
        static String EliminateOcc(String Transaction)
        {
            String[] Items = Transaction.Split(',');
            String re = "";
            Dictionary<String, int> _temp = new Dictionary<string, int>();
            foreach (String item in Items)
                if (!_temp.ContainsKey(item))
                    _temp.Add(item, 0);
            foreach (KeyValuePair<String, int> item in _temp)
                if (re == "")
                    re += item.Key;
                else
                    re += "," + item.Key;

            return re;


        }
        static List<String> GenerateSubsets(String Item)
        {


            String firstItem = Item.Split(',')[0];
            List<String> mySubsets = new List<string>();
            mySubsets.Add("");
            mySubsets.Add(firstItem);
            Boolean firstEntrance = true;
            foreach (String I in Item.Split(','))
            {
                List<String> _tempSet = new List<string>();
                // _tempSet.CopyTo(mySubsets.ToArray()); 
                for (int i = 0; i < mySubsets.Count; i++)
                {
                    _tempSet.Add(mySubsets.ElementAt(i));

                }
                //Escape first item
                if (firstEntrance) { firstEntrance = false; continue; }
                //Phase 1
                for (int i = 0; i < mySubsets.Count; i++)
                {
                    if (mySubsets.ElementAt(i).Length == 0)
                        mySubsets[i] = I;
                    else
                        mySubsets[i] += "," + I;

                }
                //Phase 2 CONCATENATION
                mySubsets.AddRange(_tempSet);

            }
            List<String> finalSubsets = new List<string>();
            foreach (String sub in mySubsets)
            {
                if (sub.Split(',').Count() != Item.Split(',').Count() && sub != "")
                    finalSubsets.Add(sub);
            }
            return finalSubsets;

        }

        static void GeneratingRules(Dictionary<string, int> Items)
        {
            bool found = false;
            Console.WriteLine("-----------Association Rules -----------");
            foreach (KeyValuePair<string, int> Item in Items)
            {
                //GENERATE SUBSETS
                List<String> mySubsets = GenerateSubsets(Item.Key);
                // CALCULATE RULES
                for (int i = 0; i < mySubsets.Count; i++)
                {
                    for (int j = i; j < mySubsets.Count; j++)
                    {

                        //CHECK s -> (l-s)
                        if (mySubsets[i].Split(',').Count() + mySubsets[j].Split(',').Count() != Item.Key.Split(',').Count())
                            continue;
                        String XandY = mySubsets[i] + "," + mySubsets[j];
                        if (EliminateOcc(XandY).Split(',').Count() != Item.Key.Split(',').Count())
                            continue;

                        float iC = float.Parse("" + getOCCItem(XandY)) / float.Parse("" + getOCCItem(mySubsets[i]));
                        if (iC >= confidence)
                        {
                            Console.WriteLine("|        " + mySubsets[i] + " --> " + mySubsets[j] + "  conf = " + getOCCItem(XandY) + "/ " + getOCCItem(mySubsets[i]) + "         |");
                            found = true;
                        }
                        float jC = float.Parse("" + getOCCItem(XandY)) / float.Parse("" + getOCCItem(mySubsets[j]));

                        if (jC >= confidence)
                        {
                            Console.WriteLine("|        " + mySubsets[j] + " --> " + mySubsets[i] + "  conf = " + getOCCItem(XandY) + "/ " + getOCCItem(mySubsets[j]) + "         |");
                            found = true;

                        }

                    }
                }


            }
            if (!found)
                Console.WriteLine("No rules were found");
            else
                Console.WriteLine("-----------Association Rules-----------");
        }
        static int getOCCItem(String Tran)
        {
            Dictionary<string, int> _TempCount = new Dictionary<string, int>();

            foreach (String Line in Transactions)
            {
                string LineItem = Line.Trim();
                if (LineItem.Split(' ').Count() < Tran.Split(',').Count())
                    continue;
                int cptLine = 0;
                //COMPARE BETWEEN COMIBINITION AND LINE TRANSACTION
                foreach (String ItemC in Tran.Split(','))
                {
                    foreach (String ItemL in Line.Trim().Split(' '))
                    {
                        if (ItemC.Equals(ItemL)) { cptLine++; continue; }
                    }

                }
                if (cptLine == Tran.Split(',').Count())
                    if (_TempCount.ContainsKey(Tran))
                        _TempCount[Tran] += 1;
                    else
                        _TempCount.Add(Tran, 1);

            }
            return _TempCount[Tran];
        }
        static int min_sup = 0;
        static float confidence = 0;
        static void Apriori()
        {

            if (Transactions.Count() == 0)
            {
                Console.WriteLine("Empty file..!");
                return;

            }
            //GET C1
            Dictionary<string, int> C1 = getC1(Transactions);
            Console.WriteLine("Support Count Average = " + getAverage(C1));
            Console.Write("Enter a minimum support = ");
            min_sup = int.Parse(Console.ReadLine());
            Dictionary<string, int> L1 = getLN(C1);
            Console.WriteLine("--------L1----------");
            foreach (KeyValuePair<string, int> i in L1)
            {
                Console.WriteLine("{ " + i.Key + " } | " + i.Value);
            }
            Dictionary<string, int> OLD_C = C1;
            Dictionary<string, int> OLD_L = L1;
            // OLD_Ls.Add(1, L1);

            Dictionary<string, int> NEW_C;
            Dictionary<string, int> NEW_L;
            int cpt = 2;
            int cptL = 1;
            while (true)
            {
                NEW_C = getCN(OLD_L, cpt);
                NEW_L = getLN(NEW_C);
                Console.WriteLine("--------L" + cpt + "----------");
                foreach (KeyValuePair<string, int> i in NEW_L)
                {
                    Console.WriteLine("{ " + i.Key + " } | " + i.Value);
                }
                if (NEW_L.Count() == 0)
                {
                    Console.Write("Enter Confidence (e.g. 0,7)= ");
                    confidence = float.Parse(Console.ReadLine());
                    //confidence /= 10;
                    GeneratingRules(OLD_L);
                    break;

                }
                else
                {
                    OLD_C = NEW_C;
                    OLD_L = NEW_L;


                    cptL++;
                    cpt++;
                }


            }




        }
        //---------------------- Apriori END ----------------

        //---------------------- FP GROWTH BEGING -----------
        static void Preprocessing()
        {
            String[] _tempTransaction = new String[Transactions.Count()];
            for (int i = 0; i < _tempTransaction.Count(); i++)
            {
                int Attr = 1;
                foreach (String Attribute in Transactions[i].Split(' '))
                {
                    if (String.IsNullOrEmpty(_tempTransaction[i]))
                        _tempTransaction[i] = "C" + Attr + "_" + Attribute;
                    else
                        _tempTransaction[i] += " " + "C" + Attr + "_" + Attribute;
                    Attr++;
                }
            }


            Transactions = _tempTransaction;

        }
        static void FP_Growth()
        {
            if (Transactions.Count() == 0)
            {
                Console.WriteLine("Empty file..!");
                return;

            }
            //GET C1
            Dictionary<string, int> C1 = getC1(Transactions);
            Console.WriteLine("Support Count Average = " + getAverage(C1));
            Console.Write("Enter a minimum support = ");
            min_sup = int.Parse(Console.ReadLine());
            Dictionary<string, int> L1 = getLN(C1);
            // Order By Descendent
            L1 = L1.OrderByDescending(Item => Item.Value).ToDictionary(Item => Item.Key, Item => Item.Value);
            Console.WriteLine("--------L1----------");
            foreach (KeyValuePair<string, int> i in L1)
            {
                Console.WriteLine("{ " + i.Key + " } | " + i.Value);
            }
            // Return if nothing > than the min_sup criteria
            if (L1.Count == 0) { Console.WriteLine("All Transactions have been eliminated"); return; }
            // Get ordered list
            String[] OrderedItemSet = getOrderedItems(L1);
            // Dump the OrderedList
            dumpOrderedList(OrderedItemSet);
            //Creating Tree and Items Routers
            Node root = new Node();
            Dictionary<String, Node> ItemsRouters = new Dictionary<string, Node>();
            InitRouters(ItemsRouters, L1);
            // Construct Tree
            TreeConstruction(ItemsRouters, root, OrderedItemSet);
            // Order By Ascendent
            L1 = L1.OrderBy(Item => Item.Value).ToDictionary(Item => Item.Key, Item => Item.Value);

            // Conditional Pattern Base
            String[] ConPatternBase = new string[L1.Count];
            getCondPatternBase(L1, ConPatternBase, ItemsRouters);
            String[] ConFPTree = new string[L1.Count];
            getConFPTree(ConPatternBase, ConFPTree, L1);
            Console.Write("Enter Confidence (e.g. 0,7)= ");
            confTree = double.Parse(Console.ReadLine());
            //confTree /= 10;

            FrequentPatternGeneration(ConFPTree, L1);


        }
        static bool foundTree;
        static double confTree = 0;
        static void FrequentPatternGeneration(String[] ConFPTree, Dictionary<String, int> L1)
        {

            int cpt = 0;
            Console.WriteLine("-----------Association Rules-----------");
            foreach (string Condition in ConFPTree)
            {
                if (String.IsNullOrEmpty(Condition.Split(':')[0]))
                    continue;
                //------- generate Subs using concatenation of (L1{i} and Condition{i})

                List<String> GeneratedSemiSubs = GenerateSemiSubsets(Condition.Split(':')[0] + "," + L1.ElementAt(cpt).Key);
                //--------- check association rules in each GeneratedSemiSubs Sets

                foreach (String Item in GeneratedSemiSubs)
                {
                    GeneratingRulesTree(Item, int.Parse(Condition.Split(':')[1]), confTree);
                }
                cpt++;
            }
            if (!foundTree)
                Console.WriteLine("No rules was found");
            else
                Console.WriteLine("-----------Association Rules-----------");

        }
        static List<String> GenerateSemiSubsets(String Item)
        {

            String IndexItem = Item.Split(',')[Item.Split(',').Count() - 1];
            String firstItem = Item.Split(',')[0];
            List<String> mySubsets = new List<string>();
            mySubsets.Add("");
            mySubsets.Add(firstItem);
            Boolean firstEntrance = true;
            foreach (String I in Item.Split(','))
            {
                List<String> _tempSet = new List<string>();
                // _tempSet.CopyTo(mySubsets.ToArray()); 
                for (int i = 0; i < mySubsets.Count; i++)
                {
                    _tempSet.Add(mySubsets.ElementAt(i));

                }
                //Escape first item
                if (firstEntrance) { firstEntrance = false; continue; }
                //Phase 1
                for (int i = 0; i < mySubsets.Count; i++)
                {
                    if (mySubsets.ElementAt(i).Length == 0)
                        mySubsets[i] = I;
                    else
                        mySubsets[i] += "," + I;

                }
                //Phase 2 CONCATENATION
                mySubsets.AddRange(_tempSet);

            }
            List<String> finalSubsets = new List<string>();
            foreach (String sub in mySubsets)
            {
                if (sub != "" && sub.Split(',').Count() != 1 && ExistItemSet(IndexItem, sub))
                    finalSubsets.Add(sub);
            }
            return finalSubsets;

        }
        static void GeneratingRulesTree(String Item, int CountSup, double confidence)
        {

            //GENERATE SUBSETS
            List<String> mySubsets = GenerateSubsets(Item);

            // CALCULATE RULES
            for (int i = 0; i < mySubsets.Count; i++)
            {
                for (int j = i; j < mySubsets.Count; j++)
                {
                    //CHECK s -> (l-s)
                    if (mySubsets[i].Split(',').Count() + mySubsets[j].Split(',').Count() != Item.Split(',').Count())
                        continue;
                    String XandY = mySubsets[i] + "," + mySubsets[j];
                    if (EliminateOcc(XandY).Split(',').Count() != Item.Split(',').Count())
                        continue;

                    float iC = float.Parse("" + CountSup) / float.Parse("" + getOCCItem(mySubsets[i]));
                    if (iC >= confidence)
                    {
                        Console.WriteLine("|        " + mySubsets[i] + " --> " + mySubsets[j] + "  conf = " + CountSup + "/ " + getOCCItem(mySubsets[i]) + "         |");
                        foundTree = true;
                    }
                    float jC = float.Parse("" + CountSup) / float.Parse("" + getOCCItem(mySubsets[j]));

                    if (jC >= confidence)
                    {
                        foundTree = true;
                        Console.WriteLine("|        " + mySubsets[j] + " --> " + mySubsets[i] + "  conf = " + CountSup + "/ " + getOCCItem(mySubsets[j]) + "         |");

                    }

                }
            }




        }
        static void getConFPTree(String[] ConPatternBase, String[] ConFPTree, Dictionary<String, int> d)
        {
            Console.WriteLine("--------Conditional FP Tree----------");

            Dictionary<String, int> Count = new Dictionary<string, int>();
            int cpt = 0;
            int _supPerPattern = 0;
            foreach (String Pattern in ConPatternBase)
            {
                ConFPTree[cpt] = "";
                Count.Clear();
                _supPerPattern = 0;
                foreach (string Term in Pattern.Split('#'))
                {
                    if (String.IsNullOrEmpty(Term.Split(':')[0]))
                        continue;
                    _supPerPattern += int.Parse(Term.Split(':')[1]);
                    foreach (String Item in Term.Split(':')[0].Split(','))
                    {
                        if (Count.ContainsKey(Item))
                            Count[Item] += 1;
                        else
                            Count.Add(Item, 1);
                    }
                }
                foreach (KeyValuePair<string, int> Item in Count)
                {
                    if (Pattern.Split('#').Count() == Item.Value)
                    {
                        if (ConFPTree[cpt] == "")
                            ConFPTree[cpt] = Item.Key;
                        else
                            ConFPTree[cpt] += "," + Item.Key;

                    }
                }
                if (ConFPTree[cpt].Length > 0)
                    ConFPTree[cpt] += ":" + _supPerPattern;
                if (ConFPTree[cpt] != "")
                    Console.WriteLine("'" + d.ElementAt(cpt).Key + "' --->  " + ConFPTree[cpt]);
                cpt++;

            }

        }
        static void getCondPatternBase(Dictionary<string, int> L, String[] ConPatternBase, Dictionary<String, Node> Routers)
        {
            int cpt = 0;
            foreach (KeyValuePair<string, int> Item in L)
            {

                ConPatternBase[cpt] = getPATHndSUP(Item.Key, Routers);
                Console.WriteLine("Conditional Pattern Base for '" + Item.Key + "' ---> " + ConPatternBase[cpt]);
                cpt++;
            }
        }
        static string getPATHndSUP(String item, Dictionary<String, Node> Routers)
        {
            String re = "";
            String _re = "";
            Node _tempAccess = Routers[item];
            //Console.WriteLine(_tempAccess);

            //Fetching tokens
            while (_tempAccess != null)
            {
                re = "";
                //Fetching each token path
                Node _tempPath = _tempAccess.PreviousItem;
                while (!_tempPath.Item.Equals("root"))
                {
                    if (re == "")
                    {
                        re = _tempPath.Item;
                    }
                    else
                    {
                        re = _tempPath.Item + "," + re;
                    }
                    _tempPath = _tempPath.PreviousItem;
                }
                // Console.WriteLine(re);


                _re = _re + "#" + re + ":" + _tempAccess.weight;

                _tempAccess = _tempAccess.NextSame;
            }

            return _re.Substring(1);
        }
        static String[] getOrderedItems(Dictionary<string, int> L)
        {
            Console.WriteLine("--------Ordered Itemset----------");

            String[] _temp = new string[Transactions.Count()];
            foreach (KeyValuePair<string, int> Item in L)
            {
                for (int i = 0; i < Transactions.Count(); i++)
                {
                    if (ExistItem(Item.Key, Transactions[i]))
                    {
                        if (_temp[i] == "" || _temp[i] == null)
                            _temp[i] = Item.Key;
                        else
                            _temp[i] += "," + Item.Key;

                    }
                }
            }

            return _temp;
        }
        static Boolean ExistItem(String Item, String Transaction)
        {
            foreach (String ItemC in Transaction.Split(' '))
            {
                if (ItemC.Equals(Item))
                    return true;

            }
            return false;
        }
        static Boolean ExistItemSet(String Item, String Set)
        {
            foreach (String ItemC in Set.Split(','))
            {
                if (ItemC.Equals(Item))
                    return true;

            }
            return false;
        }
        static void dumpOrderedList(String[] OrderedItemSet)
        {
            foreach (String Line in OrderedItemSet)
            {
                if (!String.IsNullOrEmpty(Line))
                    Console.WriteLine(Line);

            }
        }
        static void TreeConstruction(Dictionary<String, Node> Routers, Node root, String[] OrderedItemSets)
        {
            Console.WriteLine("-----------Tree Construction----------");
            foreach (string Items in OrderedItemSets)
            {
                if (String.IsNullOrEmpty(Items))
                    continue;

                Node _tempRoot = root;
                foreach (string Item in Items.Split(','))
                {

                    Node _tempNode = nodeExist(_tempRoot, Item);

                    if (_tempNode != null)
                    {
                        _tempNode.weight += 1;
                        _tempRoot = _tempNode;
                        Console.WriteLine("Add weight to " + _tempNode.Item + " , weight = " + _tempNode.weight);
                    }
                    else
                    {

                        Node newNode = new Node();
                        //Add item name
                        newNode.Item = Item;
                        // add previous node
                        newNode.PreviousItem = _tempRoot;
                        //add Node to the Routers table
                        addToRouters(Routers, newNode);
                        //add Node to the nextItems of its parent
                        _tempRoot.NextItem.Add(newNode);
                        // make the newNode the root for the current Transaction
                        Console.WriteLine("Add Item " + newNode.Item + " its previous is " + _tempRoot.Item + " with weight = " + _tempRoot.weight);
                        _tempRoot = newNode;
                        // assign 1 to its weight
                        _tempRoot.weight = 1;


                    }


                }
                Console.WriteLine("----------------------------");
            }

        }
        static void addToRouters(Dictionary<String, Node> Routers, Node newNode)
        {

            //Console.WriteLine(newNode.Item);
            Node lastNode = Routers[newNode.Item];
            if (lastNode == null)
            {
                Routers[newNode.Item] = newNode;
                return;
            }

            Node _temp = null;
            while (lastNode != null)
            {
                _temp = lastNode;
                lastNode = lastNode.NextSame;
            }
            _temp.NextSame = newNode;


        }
        static Node nodeExist(Node _tempRoot, String Item)
        {
            if (_tempRoot.NextItem.Count == 0)
                return null;

            foreach (Node Next in _tempRoot.NextItem)
            {
                if (Next.Item.Equals(Item))
                    return Next;
            }
            return null;
        }
        static void InitRouters(Dictionary<String, Node> Routers, Dictionary<string, int> L)
        {
            foreach (KeyValuePair<string, int> Item in L)
                Routers.Add(Item.Key, null);
        }
        //---------------------- FP GROWTH END ----------------

        static string[] Transactions;
        static void Main(string[] args)
        {

              if (args.Count() < 1)
              {
                  Console.WriteLine("Missing Arguments!");
                  Console.ReadKey();
                  return;
              }
              //Reading Transactions --File--
              Transactions = File.ReadAllLines(@args[0]);
              for (int i = 0; i < Transactions.Count(); i++)
                  Transactions[i] = Transactions[i].Trim();
              Console.Write("Do you wish to apply preprocessing ? (Y): ");
              if (Console.ReadLine() == "Y")
                  Preprocessing();

              try
              {
                  Console.WriteLine("-----------------------");
                  Console.WriteLine("|     1- Apriori       |");
                  Console.WriteLine("|     2- FP growth     |");
                  Console.WriteLine("-----------------------");
                  Console.Write("Choose an algorithm : ");
                  if (int.Parse(Console.ReadLine()) == 1)
                  {
                      // Apriori Section
                      Apriori();
                  }
                  else
                  {
                      // FP growth Section
                      FP_Growth();
                  }






              }
              catch (Exception z)
              {
                  Console.WriteLine(z.Message);
                  Console.ReadKey();
                  return;
              }
              
          

            Console.ReadKey();

        }
    }
}
