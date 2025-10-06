using unsafe CharPointer = char*;
using Matrix = double[,];
using MatrixJ = double[][];
using PointF = (float X, float Y);
using Speed = float?;

namespace Tester.Modules;

[Order(4)]
internal class Usings(DemoRunner demo) : Runnable(demo)
{
    public unsafe override void Run()
    {
        PointF p = (1.1f, 2.2f);
        MatrixJ aj = [[1, 2, 3], [4, 5, 6], [7, 8, 9]];
        Matrix a = new[,]
        {
            { 1.1, 2.2, 3.3 },
            { 4.4, 5.5, 6.6 },
            { 7.7, 8.8, 9.9 }
        };
        Speed s = 111.1f;

        char c = 'A';
        CharPointer pointer = &c;

        Dump(p);
        Dump(aj, "Jagged array");
        Dump(a, "2D array");
        Dump(s);

        Dump($"0x{(long)pointer:X}", "Pointer address");
    }
}