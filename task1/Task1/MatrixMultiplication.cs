using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hw1
{
    public class MatrixMultiplication1
    {
        public static int[,] Multiplication(int[,] a, int[,] b)
        {
            var t1 = DateTime.Now;
            if (a == null || b == null)
                return null;
            //if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
            if (a.Length / a.GetLength(0) != b.GetLength(0))
            {
                Console.WriteLine("Матрицы нельзя перемножить");
                //int[,] empty;
                // Возврат пустой матрицы
                return null;
            }
            int[,] r = new int[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                //counterTemp1++;
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    for (int k = 0; k < b.GetLength(0); k++)
                    {
                        r[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            var t2 = DateTime.Now - t1;
            Console.WriteLine("Без многопоточки:");
            Console.WriteLine(t2);
            Console.WriteLine(r[0, 0]);
            return r;
        }

        //Функция, умножающая две матрицы (многопоточная)
        public static int[,] MultiplicationConcurent(int[,] a, int[,] b)
        {
            var t1 = DateTime.Now;
            //if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
            if (a == null || b == null)
                return null;
            if (a.Length / a.GetLength(0) != b.GetLength(0))
            {
                Console.WriteLine("Матрицы нельзя перемножить");
                //int[,] empty;
                // Возврат пустой матрицы
                return null;
            }
            int[,] r = new int[a.GetLength(0), b.GetLength(1)];
            var threads = new Thread[12];
            var chunkSize = (b.GetLength(1)) / threads.Length + 1;
            for (var m = 0; m < threads.Length; m++)
            {
                var locall = m;
                threads[m] = new Thread(() =>
                {
                    for (int i = 0; i < a.GetLength(0); i++)
                    {
                        for (var j = locall * chunkSize; j < (locall + 1) * chunkSize && j < b.GetLength(1); j++)
                        {
                            for (int k = 0; k < b.GetLength(0); k++)
                            {
                                r[i, j] += a[i, k] * b[k, j];

                            }
                        }
                    }
                });
            }
            foreach (var thread in threads)
                thread.Start();
            foreach (var thread in threads)
                thread.Join();

            var t2 = DateTime.Now - t1;
            Console.WriteLine("C многопоточкой:");
            Console.WriteLine(t2);
            //Console.WriteLine(r);
            Console.WriteLine(r[0, 0]);
            return r;
        }
    }
}
