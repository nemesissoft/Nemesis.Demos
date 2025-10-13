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

        HighlightDecompiledCSharp(nameof(Examples));

        HighlightDecompiledMsil(nameof(Examples));

        RenderBenchmark("""
    Method;Job;AnalyzeLaunchVariance;EvaluateOverhead;MaxAbsoluteError;MaxRelativeError;MinInvokeCount;MinIterationTime;OutlierMode;Affinity;EnvironmentVariables;Jit;LargeAddressAware;Platform;PowerPlanMode;Runtime;AllowVeryLargeObjects;Concurrent;CpuGroups;Force;HeapAffinitizeMask;HeapCount;NoAffinitize;RetainVm;Server;Arguments;BuildConfiguration;Clock;EngineFactory;NuGetReferences;Toolchain;IsMutator;InvocationCount;IterationCount;IterationTime;LaunchCount;MaxIterationCount;MaxWarmupIterationCount;MemoryRandomization;MinIterationCount;MinWarmupIterationCount;RunStrategy;UnrollFactor;WarmupCount;Mean;Error;StdDev;Ratio;RatioSD;Allocated;Alloc Ratio
    "'Old lock (Monitor, parallel)'";Job-YFEFPZ;False;Default;Default;Default;Default;Default;Default;111111111111;Empty;RyuJit;Default;X64;8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c;.NET 9.0;False;True;False;True;Default;Default;False;False;False;Default;Default;Default;Default;Default;Default;Default;Default;10;Default;Default;Default;Default;Default;Default;Default;Default;16;3;"568,767.00 ns";"18,607.707 ns";"12,307.846 ns";baseline;;4650 B;
    "'New lock (Lock struct, parallel)'";Job-YFEFPZ;False;Default;Default;Default;Default;Default;Default;111111111111;Empty;RyuJit;Default;X64;8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c;.NET 9.0;False;True;False;True;Default;Default;False;False;False;Default;Default;Default;Default;Default;Default;Default;Default;10;Default;Default;Default;Default;Default;Default;Default;Default;16;3;"583,163.83 ns";"30,206.424 ns";"19,979.679 ns";+3%;3.9%;4650 B;+0%
    'Old lock (Monitor)';Job-YFEFPZ;False;Default;Default;Default;Default;Default;Default;111111111111;Empty;RyuJit;Default;X64;8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c;.NET 9.0;False;True;False;True;Default;Default;False;False;False;Default;Default;Default;Default;Default;Default;Default;Default;10;Default;Default;Default;Default;Default;Default;Default;Default;16;3;15.18 ns;0.072 ns;0.043 ns;baseline;;0 B;NA
    'New lock (Lock struct)';Job-YFEFPZ;False;Default;Default;Default;Default;Default;Default;111111111111;Empty;RyuJit;Default;X64;8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c;.NET 9.0;False;True;False;True;Default;Default;False;False;False;Default;Default;Default;Default;Default;Default;Default;Default;10;Default;Default;Default;Default;Default;Default;Default;Default;16;3;13.26 ns;0.213 ns;0.126 ns;-13%;0.9%;0 B;NA
    """);
    }

    public static void Examples()
    {
        object oldLock = new();
        Lock newLock = new();


        int i = 0;
        lock (oldLock)
        {
            i++;
        }



        lock (newLock)
        {
            i++;
        }
    }
}