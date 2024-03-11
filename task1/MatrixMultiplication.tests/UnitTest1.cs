using hw1;
using System.Numerics;

namespace MatrixMultiplication.tests;

public class Tests
{
    [Test]
    public void NonMutptiplicable()
    {
        int[,] a = { { 1, 2 },
                     {4, 1 } };



        int[,] b = { { 1, 2 },
                     {4, 1 },
                     {5, 6 } };

        int[,] c = MatrixMultiplication1.Multiplication(a, b);
    }
}