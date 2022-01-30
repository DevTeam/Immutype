namespace Immutype.Benchmark;

using System.Reflection;
using BenchmarkDotNet.Running;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run(Assembly.GetExecutingAssembly());
    }
}