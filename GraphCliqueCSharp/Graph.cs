using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphCliqueCSharp
{
    public class MaxCliqueGraph // MyGraph
    {
        private BitMatrix data;
        private int numberNodes;
        private int numberEdges;
        private int[] numberNeighbors;

        public MaxCliqueGraph(string graphFile, string fileFormat)
        {
            if (fileFormat.ToUpper() == "DIMACS") //Discrete Mathematics and Theoretical Computer Science
                LoadDimacsFormatGraph(graphFile);
            else
                throw new Exception("Format " + fileFormat + " not supported");
        }

        private void LoadDimacsFormatGraph(string graphFile)
        {
            FileStream ifs = new FileStream(graphFile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string line = "";
            string[] tokens = null;

            line = sr.ReadLine();
            line = line.Trim();
            while (line != null && line.StartsWith("p") == false)
            {
                line = sr.ReadLine();
                line = line.Trim();
            }

            tokens = line.Split(' ');
            int numNodes = int.Parse(tokens[2]); // Convert.ToInt32
            int numEdges = int.Parse(tokens[3]);
            if (numNodes < 0 || numEdges < 0)
                throw new Exception("Number nodes or edges is a negative");
            sr.Close();
            ifs.Close();
            this.data = new BitMatrix(numNodes);

            //Технически — из-за наличия строки «p» до любой строки «e» — нет никакой нужды дважды читать файл формата DIMACS.
            //Однако для других форматов файлов, которые не хранят в явном виде количество ребер,
            //вам может понадобиться двойной проход по аналогии с примененным здесь.

            ifs = new FileStream(graphFile, FileMode.Open);
            sr = new StreamReader(ifs);

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("e") == true)
                {
                    tokens = line.Split(' ');
                    int nodeA = int.Parse(tokens[1]) - 1;
                    int nodeB = int.Parse(tokens[2]) - 1;
                    if (nodeA < 0 || nodeA > numNodes - 1 || nodeB < 0 || nodeB > numNodes - 1)
                        throw new Exception("The node parameter is not in the range 0...this.numberNodes-1");
                    data.SetValue(nodeA, nodeB, true);
                    data.SetValue(nodeB, nodeA, true);
                }
            }

            sr.Close();
            ifs.Close();

            this.numberNeighbors = new int[numNodes];
            for (int row = 0; row < numNodes; ++row)
            {
                int count = 0;
                for (int col = 0; col < numNodes; ++col)
                {
                    if (data.GetValue(row, col) == true) ++count;
                }

                numberNeighbors[row] = count;
            }

            this.numberNodes = numNodes;
            this.numberEdges = numEdges;
            return;
        }

        public int NumberNodes
        {
            get { return this.numberNodes; }
        }

        public int NumberEdges
        {
            get { return this.numberEdges; }
        }

        public int NumberNeighbors(int node)
        {
            return this.numberNeighbors[node];
        }

        public bool AreAdjacent(int nodeA, int nodeB)
        {
            if (this.data.GetValue(nodeA, nodeB) == true)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            /*
            string s = "";
            for (int i = 0; i < this.data.Dim; ++i)
            {
                s += i + ": ";
                for (int j = 0; j < this.data.Dim; ++j)
                {
                    if (this.data.GetValue(i, j) == true)
                        s += j + " ";
                }
                s += Environment.NewLine;
            }
            return s;
            */

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < this.data.Dim; ++i)
            {
                sb.Append(i + ": ");
                for (int j = 0; j < this.data.Dim; ++j)
                {
                    if (this.data.GetValue(i, j) == true)
                        sb.Append(j + " ");
                }

                sb.Append(Environment.NewLine); // вместо «\n», что позволяет сделать мой класс Graph более портируемым.
            }

            return sb.ToString();
        }

        /// <summary>
        /// Проверка файла данных графа до создания экземпляра объекта графа
        /// </summary>
        public static void ValidateGraphFile(string graphFile, string fileFormat)
        {
            if (fileFormat.ToUpper() == "DIMACS")
                ValidateDimacsGraphFile(graphFile);
            else
                throw new Exception("Format " + fileFormat + " not supported");
        }


        public static void ValidateDimacsGraphFile(string graphFile)
        {
            FileStream ifs = new FileStream(graphFile, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string line = "";
            string[] tokens = null;

            line = sr.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("c") == false &&
                    line.StartsWith("p") == false &&
                    line.StartsWith("e") == false)
                    throw new Exception("Unknown line type: " + line);

                try
                {
                    if (line.StartsWith("p"))
                    {
                        tokens = line.Split(' ');
                        int numNodes = int.Parse(tokens[2]);
                        int numEdges = int.Parse(tokens[3]);
                    }

                    if (line.StartsWith("e"))
                    {
                        tokens = line.Split(' ');
                        int nodeA = int.Parse(tokens[1]) - 1;
                        int nodeB = int.Parse(tokens[2]) - 1;
                    }
                }
                catch
                {
                    throw new Exception("Error parsing line = " + line);
                }

                line = sr.ReadLine();
            }

            sr.Close();
            ifs.Close();
        }

        public void ValidateGraph()
        {
            for (int i = 0; i < this.data.Dim; ++i)
            {
                for (int j = 0; j < this.data.Dim; ++j)
                {
                    if (this.data.GetValue(i, j) != this.data.GetValue(j, i))
                        throw new Exception("Not symmetric at " + i + " and " + j);
                }
            }

            for (int i = 0; i < this.data.Dim; ++i)
            {
                if (this.data.GetValue(i, i) == true)
                    throw new Exception("The presence of a true/1 on the bit matrix main diagonal in row " + i);
            }

            bool isAllFalse = true;
            bool isAllTrue = true;
            bool isOk = false;

            for (int i = 0; i < this.data.Dim && !isOk; ++i)
            {
                for (int j = 0; j < this.data.Dim; ++j)
                {
                    if (this.data.GetValue(i, j) == true)
                        isAllFalse = false;
                    else
                        isAllTrue = false;

                    isOk = !(isAllFalse || isAllTrue);
                }
            }

            if (!isOk)
            {
                if (isAllFalse)
                    throw new Exception("A bit matrix consisting of either all false (0)");
                if (isAllTrue)
                    throw new Exception("A bit matrix consisting of either all true (1)");
            }

            int sumNumberNeighbors = 0;
            foreach (var numberNeighbor in numberNeighbors)
            {
                sumNumberNeighbors += numberNeighbor;
            }

            int totalNumberOfTrueValues = 0;
            for (int i = 0; i < this.data.Dim; ++i)
            {
                for (int j = 0; j < this.data.Dim; ++j)
                {
                    if (this.data.GetValue(i, j) == true)
                        totalNumberOfTrueValues += 1;
                }
            }

            if (sumNumberNeighbors != totalNumberOfTrueValues)
                throw new Exception("The sum of the values in the numberNeighbors array field" +
                                    " not equaling the total number of true/1 values in the bit matrix");
        }


        // -------------------------------------------------------------------
        private class BitMatrix
        {
            private BitArray[] data;
            public readonly int Dim;

            public BitMatrix(int n)
            {
                this.data = new BitArray[n];
                for (int i = 0; i < data.Length; ++i)
                {
                    this.data[i] = new BitArray(n);
                }
                this.Dim = n;
            }
            public bool GetValue(int row, int col)
            {
                return data[row][col];
            }
            public void SetValue(int row, int col, bool value)
            {
                data[row][col] = value;
            }
            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < data.Length; ++i)
                {
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        if (data[i][j] == true)
                            s += "1 ";
                        else
                            s += "0 ";
                    }
                    s += Environment.NewLine;
                }
                return s;
            }
        }
        // -------------------------------------------------------------------

    } // класс MaxCliqueGraph
}
