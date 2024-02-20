using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;


// Считывание первой матрицы:
Console.WriteLine("Введите путь для первой матрицы:");
string path = Console.ReadLine();
path ??= "";
StreamReader sr = new StreamReader(path + "\\matrix1.txt"); // "C:\\Users\\User\\source\\repos\\mm1\\mm1"

var data = sr.ReadToEnd();

string[] dataSplit = data.Split();

int r1 = int.Parse(dataSplit[0]);
int c1 = int.Parse(dataSplit[1]);
int counterTemp = 2;
int[,] matrix1 = new int[r1, c1];

for (int i = 0; i < r1;)
{
    for(int j = 0; j < c1;)
    {
        if (dataSplit[counterTemp] != "\n" && dataSplit[counterTemp] != " " && dataSplit[counterTemp] != "\r" && dataSplit[counterTemp] != "")
        {
            matrix1[i, j] = int.Parse(dataSplit[counterTemp]);
            j++;
        }
        counterTemp++;
    }
    i++;
}

sr.Close();

// Считывание второй матрицы:
Console.WriteLine("Введите путь для первой матрицы:");
path = Console.ReadLine();
path ??= "";
sr = new StreamReader(path + "\\matrix2.txt"); // "C:\\Users\\User\\source\\repos\\mm1\\mm1"

data = sr.ReadToEnd();

dataSplit = data.Split();

int r2 = int.Parse(dataSplit[0]);
int c2 = int.Parse(dataSplit[1]);
counterTemp = 2;

int[,] matrix2 = new int[r2, c2];

for (int i = 0; i < r2;)
{
    for (int j = 0; j < c2;)
    {
        if (dataSplit[counterTemp] != "\n" && dataSplit[counterTemp] != " " && dataSplit[counterTemp] != "\r" && dataSplit[counterTemp] != "")
        {
            matrix2[i, j] = int.Parse(dataSplit[counterTemp]);
            j++;
        }
        counterTemp++;
    }
    i++;
}

sr.Close();


//Функция, умножающая две матрицы 
static int[,] Multiplication(int[,] a, int[,] b)
{
    var t1 = DateTime.Now;
    if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
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
    return r;
}

//Функция, умножающая две матрицы (многопоточная)
static int[,] MultiplicationConcurent(int[,] a, int[,] b)
{
    var t1 = DateTime.Now;
    if (a.GetLength(1) != b.GetLength(0)) throw new Exception("Матрицы нельзя перемножить");
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
    return r;
}
// Вызов функции умножения без многопоточности
int[,] matrix3 = Multiplication(matrix1, matrix2);
// Вызов функции умножения с многопоточностью
matrix3 = MultiplicationConcurent(matrix1, matrix2);

//Сохранение итоговой матрицы:
Console.WriteLine("Введите путь сохранения матрицы:");
path = Console.ReadLine();
path ??= "";
StreamWriter sw = new StreamWriter(path + "\\matrix3.txt", true); // "C:\\Users\\User\\source\\repos\\mm1\\mm1"
sw.WriteLine(matrix3.GetLength(0) + " " + matrix3.GetLength(0));
for(int i = 0; i < matrix3.GetLength(0); i++)
{
    for (int j = 0; j < matrix3.GetLength(0); j++)
    {
        sw.Write(matrix3[i, j] + " ");
    }
    sw.WriteLine();
}

sw.Close();
