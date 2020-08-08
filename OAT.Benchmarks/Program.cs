using BenchmarkDotNet.Running;

namespace OAT.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ExpressionBenchmarks>();
        }
    }
}