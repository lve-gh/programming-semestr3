using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hw1
{
    public class MatrixWorkFile1
    {
        public static int[,] MatrixRead(string path)
        {
            StreamReader sr = null;
            if (!File.Exists(path))
            {
                Console.WriteLine("Файла нет");
                return null;
            }
            else
                sr = new StreamReader(path);
            //Console.WriteLine(11);
            var data = sr.ReadToEnd();

            string[] dataSplit = data.Split();

            int rows = int.Parse(dataSplit[0]);
            int columns = int.Parse(dataSplit[1]);
            int counterTemp = 2;
            int[,] matrix = new int[rows, columns];

            for (int i = 0; i < rows;)
            {
                for (int j = 0; j < columns;)
                {
                    if (dataSplit[counterTemp] != "\n" && dataSplit[counterTemp] != " " && dataSplit[counterTemp] != "\r" && dataSplit[counterTemp] != "")
                    {
                        matrix[i, j] = int.Parse(dataSplit[counterTemp]);
                        j++;
                    }
                    counterTemp++;
                }
                i++;
            }

            sr.Close();
            return matrix;
        }
        public static void MatrixWrite(int[,] matrix, string path)
        {
            StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(matrix.GetLength(0) + " " + matrix.GetLength(0));
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    sw.Write(matrix[i, j] + " ");
                }
                sw.WriteLine();
            }

            sw.Close();
        }
    }
}
