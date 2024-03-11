using hw1;
namespace MatrixWorkFile.tests;

public class Tests
{
    [Test]
    public void ReadNotExisted()
    {
        string path = "11111";
        MatrixWorkFile1.MatrixRead(path);
    }
    [Test]
    public void WriteCon()
    {
        int[,] a = { { 1 } };
        MatrixWorkFile1.MatrixWrite(a, "con");
    }
}