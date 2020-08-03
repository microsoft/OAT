using BenchmarkDotNet.Attributes;
using Microsoft.CST.OAT;
using Microsoft.CST.OAT.Tests;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CST.OAT.Tests.VehicleDemo;

namespace OAT.Benchmarks
{
    public class ExpressionBenchmarks
    {
        public static VehicleRule carpoolRule = new VehicleRule("Carpool Car")
        {
            Cost = 2,
            Severity = 2,
            Target = "Vehicle",
            Expression = "IsCar AND OccupantsGT2",
            Clauses = new List<Clause>()
            {
                new Clause(Operation.Equals, "VehicleType")
                {
                    Label = "IsCar",
                    Data = new List<string>()
                    {
                        "Car"
                    }
                },
                new Clause(Operation.GreaterThan, "Occupants")
                {
                    Label = "OccupantsGT2",
                    Data = new List<string>()
                    {
                        "2"
                    }
                },
            }
        };

        public static List<Rule> carpoolRules = new List<Rule>()
        {
            carpoolRule
        };

        public static List<Rule> carpoolRuleWithExtraComplexRulesThatHalfApply = new List<Rule>()
        {
            carpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule,
            notCarpoolRule
        };

        public static List<Rule> carpoolRuleWithExtraComplexRuleThatHalfApplies = new List<Rule>()
        {
            carpoolRule,
            notCarpoolRule,
        };

        public static List<Rule> carpoolRuleWithExtraRulesThatApply = new List<Rule>()
        {
            carpoolRule,
            carRule,
            carRule,
            carRule,
            carRule,
            carRule,
            carRule,
            carRule,
            carRule,
            carRule,
            carRule
        };

        public static List<Rule> carpoolRuleWithExtraRulesThatDontApply = new List<Rule>()
        {
            carpoolRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule,
            motorcycleRule
        };

        public static VehicleRule carRule = new VehicleRule("Just a Car")
        {
            Cost = 2,
            Severity = 2,
            Target = "Vehicle",
            Expression = "IsCar",
            Clauses = new List<Clause>()
            {
                new Clause(Operation.Equals, "VehicleType")
                {
                    Label = "IsCar",
                    Data = new List<string>()
                    {
                        "Car"
                    }
                }
            }
        };

        public static VehicleRule motorcycleRule = new VehicleRule("A Motorcycle")
        {
            Cost = 2,
            Severity = 2,
            Target = "Vehicle",
            Expression = "IsMotorcycle",
            Clauses = new List<Clause>()
            {
                new Clause(Operation.Equals, "VehicleType")
                {
                    Label = "IsMotorcycle",
                    Data = new List<string>()
                    {
                        "Motorcycle"
                    }
                }
            }
        };

        public static VehicleRule notCarpoolRule = new VehicleRule("Not a Carpool Car")
        {
            Cost = 2,
            Severity = 2,
            Target = "Vehicle",
            Expression = "IsCar AND OccupantsLT2",
            Clauses = new List<Clause>()
            {
                new Clause(Operation.Equals, "VehicleType")
                {
                    Label = "IsCar",
                    Data = new List<string>()
                    {
                        "Car"
                    }
                },
                new Clause(Operation.LessThan, "Occupants")
                {
                    Label = "OccupantsLT2",
                    Data = new List<string>()
                    {
                        "2"
                    }
                },
            }
        };

        public Vehicle carpoolCar = new Vehicle()
        {
            VehicleType = VehicleType.Car,
            Occupants = 3
        };

        public ExpressionBenchmarks()
        {
            Analyzer = new Analyzer();
        }

        public Analyzer Analyzer { get; }

        [Benchmark]
        public void TestCarpoolCar()
        {
            Analyzer.Analyze(carpoolRules, carpoolCar);
        }

        [Benchmark]
        public void TestCarpoolCarWithExtraRulesThatApply()
        {
            Analyzer.Analyze(carpoolRuleWithExtraRulesThatApply, carpoolCar);
        }

        [Benchmark]
        public void TestCarpoolCarWithExtraRulesThatDontApply()
        {
            Analyzer.Analyze(carpoolRuleWithExtraRulesThatDontApply, carpoolCar);
        }

        [Benchmark]
        public void TestCarpoolCarWithExtraRulesThatHalfApply()
        {
            Analyzer.Analyze(carpoolRuleWithExtraComplexRulesThatHalfApply, carpoolCar);
        }

        [Benchmark]
        public void TestCarpoolCarWithExtraRuleThatHalfApplies()
        {
            Analyzer.Analyze(carpoolRuleWithExtraComplexRuleThatHalfApplies, carpoolCar);
        }
    }
}