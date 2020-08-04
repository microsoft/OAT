using BenchmarkDotNet.Running;
using System;
using System.Linq.Expressions;

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