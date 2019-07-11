using System;
using System.Collections.Generic;
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
 */

namespace GraphCliqueCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            string graphFile = "..\\..\\DimacsGraph.clq"; //string graphFile = "DimacsGraph.clq";
            MaxCliqueGraph.ValidateGraphFile(graphFile, FileFormat.DIMACS); //"DIMACS"

            MaxCliqueGraph graph = new MaxCliqueGraph(graphFile, FileFormat.DIMACS);

            graph.ValidateGraph();
            Console.WriteLine(graph.ToString());

            Console.WriteLine("\nAre nodes 5 and 8 adjacent? " +
                              graph.AreAdjacent(5, 8));
            Console.WriteLine("Number neighbors of node 4 = " +
                              graph.NumberNeighbors(4));
        }
    }

}
