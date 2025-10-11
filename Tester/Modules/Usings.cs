using unsafe CharPointer = char*;
using Matrix = double[,];
using MatrixJ = double[][];
using MyList = System.Collections.Generic.List<string>;
using PointF = (float X, float Y);
using Speed = float?;

namespace Tester.Modules;

internal class Usings(DemoRunner demo) : Runnable(demo, order: 4)
{
    public unsafe override void Run()
    {
        Dump(new FileInfo("c:/temp/i.txt"));

        PointF p = (1.1f, 2.2f);

        MyList l = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27"];

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
        Dump(l, "List");
        Dump(aj, "Jagged array");
        Dump(a, "2D array");
        Dump(s);

        Dump($"0x{(long)pointer:X}", "Pointer address");
    }
}