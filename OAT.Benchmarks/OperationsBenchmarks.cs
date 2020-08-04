using BenchmarkDotNet.Attributes;
using Microsoft.CST.OAT;
using Microsoft.CST.OAT.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CST.OAT.Tests.VehicleDemo;

namespace OAT.Benchmarks
{
    public class OperationsBenchmarks
    {
        public static Rule regexRule = new Rule("Regex")
        {
            Clauses = new List<Clause>()
            {
                new Clause(Operation.Regex)
                {
                    Data = new List<string>()
                    {
                        "Magic"
                    }
                }
            }
        };

        public OperationsBenchmarks()
        {
            Analyzer = new Analyzer();
        }

        public Analyzer Analyzer { get; }

        [Benchmark]
        public void TestMagicString()
        {
            Analyzer.Applies(regexRule, "This String Has Magic Inside");
        }

        [Benchmark]
        public void TestNormalString()
        {
            Analyzer.Applies(regexRule, "This String Has Is Super Normal");
        }
    }
}