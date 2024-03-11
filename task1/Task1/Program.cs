using hw1;
namespace Tak1;
public class Program {
    static void Main(string[] args)
    {
        Console.WriteLine("Введите путь для первой матрицы:");
        string path = Console.ReadLine();
        path ??= "";

        int[,] matrix1 = MatrixWorkFile1.MatrixRead(path);

        Console.WriteLine("Введите путь для второй матрицы:");
        path = Console.ReadLine();
        path ??= "";

        int[,] matrix2 = MatrixWorkFile1.MatrixRead(path);

        int[,] matrix3 = MatrixMultiplication1.Multiplication(matrix1, matrix2);

        matrix3 = MatrixMultiplication1.MultiplicationConcurent(matrix1, matrix2);

        if (matrix3 != null)
        {
            for (int i = 0; i < matrix3.GetLength(0); i++)
            {
                for (int j = 0; j < matrix3.Length / matrix3.GetLength(0); j++)
                {
                    Console.WriteLine(matrix3[i, j]);
                }
            }
            Console.WriteLine("Введите путь сохранения матрицы:");
            path = Console.ReadLine();
            path ??= "";

            MatrixWorkFile1.MatrixWrite(matrix3, path);
        }
        else
            Console.WriteLine("Ошибка выполнения умножения");
    }
}