using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * 9. Найти клику в неориентированном графе
 *
 * Клика (clique) неориентированного графа G = (V,E) — это подмножество V С V вершин,
 * каждая пара в котором связана ребром из множества Е.
 * Другими словами, клика — это полный подграф графа G.
 * Размер (size) клики — это количество содержащихся в этом подграфе вершин.
 * https://msdn.microsoft.com/ru-ru/magazine/hh580741.aspx
 */

namespace GraphCliqueCSharp
{
    class GreedyMaxCliqueProgram
    {
        static Random random = null;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("\nBegin greedy maximum clique demo\n");
                Console.WriteLine("\nBegin tabu algorithm maximum clique demo\n");

                string graphFile = "..\\..\\DimacsGraph.clq"; //string graphFile = "DimacsGraph.clq";
                MaxCliqueGraph.ValidateGraphFile(graphFile, FileFormat.DIMACS); //"DIMACS"

                MaxCliqueGraph graph = new MaxCliqueGraph(graphFile, FileFormat.DIMACS);

                graph.ValidateGraph();

                Console.WriteLine("Loading graph into memory");
                Console.WriteLine("Graph loaded and validated\n");

                int maxTime = 50; // В настоящей задаче о максимальной клике maxTime обычно находится в диапазоне от 1000 до 100 000. 
                int targetCliqueSize = graph.NumberNodes;

                List<int> maxClique = FindMaxClique(graph, maxTime, targetCliqueSize);
                Console.WriteLine("\nMaximum time reached");
                Console.WriteLine("\nSize of best clique found = " + maxClique.Count);

                Console.WriteLine("\nBest clique found:");
                Console.WriteLine(ListAsString(maxClique));

                Console.WriteLine("\nEnd greedy maximum clique demo\n");

                Console.WriteLine(graph.ToString());

                Console.WriteLine("\nAre nodes 5 and 8 adjacent? " +
                                  graph.AreAdjacent(5, 8));
                Console.WriteLine("Number neighbors of node 4 = " +
                                  graph.NumberNeighbors(4));

                WriteFile(ListAsString(maxClique), "output.txt");

                string outputGraphFile = "..\\..\\output.txt";
                graph.SaveTxtFormatGraph(outputGraphFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal: " + ex.Message);
            }
        } // Main

        private static void WriteFile(string result, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine(result);
            }
        }

        static List<int> FindMaxClique(MaxCliqueGraph graph, int maxTime, int targetCliqueSize)
        {
            List<int> clique = new List<int>();
            random = new Random(1);
            int time = 0;
            int timeBestCliqueFound = 0; //timeBestClique
            int timeLastRestart = 0;//timeRestart
            int nodeToAdd = -1;
            int nodeToDrop = -1;

            int prohibitPeriod = 1;
            int timeProhibitChanged = 0;
            int[] lastMoved = new int[graph.NumberNodes];
            for (int i = 0; i < lastMoved.Length; ++i)
            {
                lastMoved[i] = int.MinValue;
            }
            Dictionary<int, CliqueInfo> history = new Dictionary<int, CliqueInfo>();

            int randomNode = random.Next(0, graph.NumberNodes);
            Console.WriteLine("Adding node " + randomNode);
            clique.Add(randomNode);

            List<int> bestClique = new List<int>();
            bestClique.AddRange(clique);
            int bestSize = bestClique.Count;
            timeBestCliqueFound = time;
            //int timeBestCliqueFound = 0;
            //int timeLastRestart = 0;

            List<int> possibleAdd = MakePossibleAdd(graph, clique);
            List<int> oneMissing = MakeOneMissing(graph, clique);

            while (time < maxTime && bestSize < targetCliqueSize)
            {
                ++time;
                bool cliqueChanged = false;
                ValidateClique(clique, possibleAdd, oneMissing);

                if (possibleAdd.Count > 0)
                {
                    List<int> allowedAdd = SelectAllowedNodes(possibleAdd, time, prohibitPeriod, lastMoved);
                    if (allowedAdd.Count > 0)
                    {
                        nodeToAdd = GetNodeToAdd(graph, allowedAdd, possibleAdd);
                        Console.WriteLine("Adding node " + nodeToAdd);
                        clique.Add(nodeToAdd);
                        lastMoved[nodeToAdd] = time;
                        clique.Sort();
                        cliqueChanged = true;
                        if (clique.Count > bestSize)
                        {
                            bestSize = clique.Count;
                            bestClique.Clear();
                            bestClique.AddRange(clique);
                            timeBestCliqueFound = time;
                        }
                    }
                } // добавление

                if (cliqueChanged == false)
                {
                    if (clique.Count > 0)
                    {
                        List<int> allowedInClique = SelectAllowedNodes(clique, time, prohibitPeriod, lastMoved);
                        if (allowedInClique.Count > 0)
                        {
                            nodeToDrop = GetNodeToDrop(graph, clique, oneMissing);
                            Console.WriteLine("Dropping node " + nodeToDrop);
                            clique.Remove(nodeToDrop);
                            lastMoved[nodeToDrop] = time;
                            clique.Sort();
                            cliqueChanged = true;
                        }
                    }
                } // удаление

                if (cliqueChanged == false) // если невозможно добавить или удалить разрешенный узел
                {
                    if (clique.Count > 0)
                    {
                        nodeToDrop = clique[random.Next(0, clique.Count)];
                        clique.Remove(nodeToDrop);
                        lastMoved[nodeToDrop] = time;
                        clique.Sort();
                        cliqueChanged = true;
                    }
                }

                int restart = 2 * bestSize; // 100
                if (time - timeBestCliqueFound > restart &&
                    time - timeLastRestart > restart)
                {
                    Console.WriteLine("\nRestarting\n");
                    timeLastRestart = time;
                    prohibitPeriod = 1;
                    timeProhibitChanged = time;
                    history.Clear();

                    int seedNode = -1;
                    List<int> temp = new List<int>();
                    for (int i = 0; i < lastMoved.Length; ++i)
                    {
                        if (lastMoved[i] == int.MinValue) temp.Add(i);
                    }
                    if (temp.Count > 0)
                        seedNode = temp[random.Next(0, temp.Count)];
                    else
                        seedNode = random.Next(0, graph.NumberNodes);

                    clique.Clear();
                    clique.Add(seedNode);
                    Console.WriteLine("Adding node " + seedNode);
                } // перезапуск

                possibleAdd = MakePossibleAdd(graph, clique); // Альтернатива — поддерживать вспомогательные структуры данных 
                oneMissing = MakeOneMissing(graph, clique); //  и обновлять эти два списка вместо повторного их создания с нуля.
                prohibitPeriod = UpdateProhibitPeriod(graph, clique, bestSize,
                    history, time, prohibitPeriod, ref timeProhibitChanged);
            } // основной цикл обработки

            return bestClique;
        }

        static List<int> MakePossibleAdd(MaxCliqueGraph graph, List<int> clique)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < graph.NumberNodes; ++i)
            {
                if (FormsALargerClique(graph, clique, i) == true)
                    result.Add(i);
            }
            return result;
        }

        static bool FormsALargerClique(MaxCliqueGraph graph, List<int> clique, int node)
        {
            for (int i = 0; i < clique.Count; ++i)
            {
                if (graph.AreAdjacent(clique[i], node) == false)
                    return false;
            }
            return true;
        }

        static int GetNodeToAdd(MaxCliqueGraph graph, List<int> allowedAdd, List<int> possibleAdd)
        {
            if (allowedAdd.Count == 1)
                return allowedAdd[0];

            List<int> allowedAddDegree = new List<int>(possibleAdd.Count);

            int maxDegree = 0;
            for (int i = 0; i < allowedAdd.Count; ++i)
            {
                int currNode = allowedAdd[i];
                int degreeOfCurrentNode = 0;
                for (int j = 0; j < possibleAdd.Count; ++j)
                {
                    int otherNode = possibleAdd[j];
                    if (graph.AreAdjacent(currNode, otherNode) == true)
                        ++degreeOfCurrentNode;
                }

                allowedAddDegree.Add(degreeOfCurrentNode);
                if (degreeOfCurrentNode > maxDegree)
                    maxDegree = degreeOfCurrentNode;
            }

            List<int> candidates = new List<int>();
            for (int i = 0; i < allowedAdd.Count; ++i)
            {
                if (allowedAddDegree[i] == maxDegree)
                    candidates.Add(possibleAdd[i]);
            }

            if (candidates.Count == 1)
                return candidates[0];

            return candidates[random.Next(0, candidates.Count)];
        }

        static int GetNodeToDrop(MaxCliqueGraph graph, List<int> clique, List<int> oneMissing)
        {
            if (clique.Count == 1)
                return clique[0];

            List<int> cliqueCountNotAdjacent = new List<int>(clique.Count);

            int maxCount = 0;
            for (int i = 0; i < clique.Count; ++i)
            {
                int currCliqueNode = clique[i];
                int countNotAdjacent = 0;
                for (int j = 0; j < oneMissing.Count; ++j)
                {
                    int currOneMissingNode = oneMissing[j];
                    if (graph.AreAdjacent(currCliqueNode,
                            currOneMissingNode) == false)
                        ++countNotAdjacent;
                }
                cliqueCountNotAdjacent.Add(countNotAdjacent);
                if (countNotAdjacent > maxCount)
                    maxCount = countNotAdjacent;
            }

            List<int> candidates = new List<int>();
            for (int i = 0; i < clique.Count; ++i)
            {
                if (cliqueCountNotAdjacent[i] == maxCount)
                    candidates.Add(clique[i]);
            }

            if (candidates.Count == 1)
                return candidates[0];

            return candidates[random.Next(0, candidates.Count)];
        } // GetNodeToDrop

        static List<int> MakeOneMissing(MaxCliqueGraph graph, List<int> clique)
        {
            int count;
            List<int> result = new List<int>();
            for (int i = 0; i < graph.NumberNodes; ++i)
            {
                if (graph.NumberNeighbors(i) < clique.Count - 1) continue;
                if (clique.BinarySearch(i) >= 0) continue;

                count = 0;
                for (int j = 0; j < clique.Count; ++j)
                {
                    if (graph.AreAdjacent(i, clique[j]))
                        ++count;
                }
                if (count == clique.Count - 1)
                    result.Add(i);
            }
            return result;
        }

        static List<int> SelectAllowedNodes(List<int> listOfNodes, int time, int prohibitPeriod, int[] lastMoved)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < listOfNodes.Count; ++i)
            {
                int currNode = listOfNodes[i];
                if (time > lastMoved[currNode] + prohibitPeriod)
                    result.Add(currNode); // разрешен
            }

            return result;
        }

        static int UpdateProhibitPeriod(MaxCliqueGraph graph, List<int> clique,
            int bestSize, Dictionary<int, CliqueInfo> history, int time, int prohibitPeriod,
            ref int timeProhibitChanged)
        {
            int result = prohibitPeriod;
            CliqueInfo cliqueInfo = new CliqueInfo(clique, time);

            if (history.ContainsKey(cliqueInfo.GetHashCode()))
            {
                CliqueInfo ci = history[cliqueInfo.GetHashCode()];

                int intervalSinceLastVisit = time - ci.LastSeen;
                ci.LastSeen = time;
                if (intervalSinceLastVisit < 2 * graph.NumberNodes - 1)
                {
                    timeProhibitChanged = time;
                    if (prohibitPeriod + 1 < 2 * bestSize) return prohibitPeriod + 1;
                    else return 2 * bestSize;
                }
            }
            else history.Add(cliqueInfo.GetHashCode(), cliqueInfo);

            if (time - timeProhibitChanged > 10 * bestSize)
            {
                timeProhibitChanged = time;
                if (prohibitPeriod - 1 > 1)
                    return prohibitPeriod - 1;
                else
                    return 1;
            }
            else
            {
                return result; // нет изменений
            }
        } // UpdateProhibitTime

        static void ValidateClique(List<int> clique, List<int> possibleAdd, List<int> oneMissing)
        {
            HashSet<int> numbers = new HashSet<int>();
            foreach (var item in clique)
            {
                numbers.Add(item);
            }
            if (numbers.Count != clique.Count)
                throw new Exception("There were duplicate nodes in the current clique");

            foreach (var node in clique)
            {
                if (possibleAdd.Contains(node))
                    throw new Exception("Node in the current clique is in the current possibleAdd list");
                if (oneMissing.Contains(node))
                    throw new Exception("Node in the current clique is in the current oneMissing list");
            }

        }

        static string ListAsString(List<int> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i] + " ");
            }

            return sb.ToString();
        }

        private class CliqueInfo
        {
            private List<int> clique;
            private int lastSeen;
            public CliqueInfo(List<int> clique, int lastSeen)
            {
                this.clique = new List<int>(clique.Count);
                this.clique.AddRange(clique);
                this.lastSeen = lastSeen;
            }
            public int LastSeen
            {
                get { return this.lastSeen; }
                set { this.lastSeen = value; }
            }
            public override int GetHashCode()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < clique.Count; ++i)
                {
                    sb.Append(clique[i]);
                    sb.Append(" ");
                }
                string s = sb.ToString();
                return s.GetHashCode();
            }
            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < clique.Count; ++i)
                    s += clique[i] + " ";
                return s;
            }
        }

    } // класс Program

} // ns
