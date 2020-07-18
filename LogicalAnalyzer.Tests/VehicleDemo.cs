using Microsoft.CST.LogicalAnalyzer.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.CST.LogicalAnalyzer.Tests
{
    [TestClass]
    public class VehicleDemo
    {
        class Vehicle
        {
            public int Weight;
            public int Axles { get; set; }
            public int Occupants { get; set; }
            public int Capacity { get; set; }
            public Driver? Driver { get; set; }
        }

        class Driver
        {
            public DriverLicense? License { get; set; }
        }

        class DriverLicense
        {
            public Endorsements Endorsements { get; set; }
            public DateTime Expiration { get; set; }
        }

        [Flags]
        enum Endorsements
        {
            Motorcycle,
            Auto,
            CDL
        }

        int GetCost(Vehicle vehicle, Analyzer analyzer, IEnumerable<Rule> rules)
        {
            // This gets the maximum severity rule that is applied and gets the cost of that rule, if no rules 0 cost
            return ((VehicleRule)analyzer.Analyze(rules, vehicle).MaxBy(x => x.Severity).FirstOrDefault())?.Cost ?? 0;
        }

        public (bool Applies, bool Result) OperationDelegate(Clause clause, IEnumerable<string>? valsToCheck, IEnumerable<KeyValuePair<string, string>> dictToCheck, object? state1, object? state2)
        {
            if (clause.CustomOperation == "OVERWEIGHT")
            {
                if (state1 is Vehicle vehicle)
                {
                    if (vehicle.Weight > vehicle.Capacity)
                    {
                        return (true, true);
                    }
                }
                return (true, false);
            }
            return (false,false);
        }

        public IEnumerable<Violation> OperationValidationDelegate(Rule r, Clause c)
        {
            if (c.CustomOperation == "OVERWEIGHT")
            {
                if (r.Target != "Vehicle")
                {
                    yield return new Violation("Overweight operation requires a Vehicle object", r, c);
                }

                if (c.Data != null || c.DictData != null)
                {
                    yield return new Violation("Overweight operation takes no data.", r, c);
                }
            }
            else
            {
                yield return new Violation($"{c.CustomOperation} is unsupported", r, c);
            }
        }

        public class VehicleRule : Rule
        {
            public int Cost;
            public VehicleRule(string name) : base(name) { }
        }

        [TestMethod]
        public void WeighStationDemo()
        {
            var truck = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                Driver = new Driver() { License = new DriverLicense() { Endorsements = Endorsements.CDL | Endorsements.Auto, Expiration = DateTime.Now.AddYears(1) } }
            };

            var overweightTruck = new Vehicle()
            {
                Weight = 30000,
                Capacity = 20000,
                Driver = new Driver() { License = new DriverLicense() { Endorsements = Endorsements.CDL | Endorsements.Auto, Expiration = DateTime.Now.AddYears(1) } }
            };

            var expiredLicense = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                Driver = new Driver() { License = new DriverLicense() { Endorsements = Endorsements.CDL | Endorsements.Auto, Expiration = DateTime.Now.AddYears(-1) } }
            };

            var noCdl = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                Driver = new Driver() { License = new DriverLicense() { Endorsements = Endorsements.Auto, Expiration = DateTime.Now.AddYears(1) } }
            };

            var rules = new VehicleRule[] {
                new VehicleRule("Overweight")
                {
                    Cost = 50,
                    Severity = 9,
                    Expression = "Overweight",
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause(OPERATION.CUSTOM)
                        {
                            Label = "Overweight",
                            CustomOperation = "OVERWEIGHT"
                        }
                    }
                },
                new VehicleRule("No CDL")
                {
                    Cost = 100,
                    Severity = 3,
                    Target = "Vehicle",
                    Expression = "NOT Has_Cdl",
                    Clauses = new List<Clause>()
                    {
                        new Clause("Driver.License.Endorsements", OPERATION.CONTAINS)
                        {
                            Label = "Has_Cdl",
                            Data = new List<string>()
                            {
                                "CDL"
                            }
                        }
                    }
                },
                new VehicleRule("Expired License"){
                    Cost = 75,
                    Severity = 1,
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause("Driver.License.Expiration", OPERATION.IS_EXPIRED)
                        {
                        }
                    }
                }
            };
            var analyzer = new Analyzer();
            analyzer.CustomOperationDelegates.Add(OperationDelegate);
            analyzer.CustomOperationValidationDelegates.Add(OperationValidationDelegate);

            var issues = analyzer.EnumerateRuleIssues(rules).ToList();

            Assert.IsFalse(issues.Any());

            Assert.IsTrue(GetCost(truck, analyzer, rules) == 0);// Compliant
            Assert.IsTrue(GetCost(overweightTruck, analyzer, rules) == 50); // Overweight
            Assert.IsTrue(GetCost(noCdl, analyzer, rules) == 100); // No CDL
            Assert.IsTrue(GetCost(expiredLicense, analyzer, rules) == 75); // Expired License
        }

        [TestMethod]
        public void TollBoothDemo()
        {
            var truck = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                Axles = 5,
                Occupants = 1
            };

            var overweightTruck = new Vehicle()
            {
                Weight = 30000,
                Capacity = 20000,
                Axles = 5,
                Occupants = 1
            };

            var car = new Vehicle()
            {
                Weight = 3000,
                Axles = 2,
                Occupants = 1
            };

            var carpool = new Vehicle()
            {
                Weight = 3000,
                Axles = 2,
                Occupants = 3
            };

            var motorcycle = new Vehicle()
            {
                Weight = 1000,
                Axles = 2,
                Occupants = 1
            };

            var rules = new VehicleRule[] {
                new VehicleRule("Overweight")
                {
                    Cost = 50,
                    Severity = 9,
                    Expression = "Overweight AND gt2Axles",
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause(OPERATION.CUSTOM)
                        {
                            Label = "Overweight",
                            CustomOperation = "OVERWEIGHT"
                        },
                        new Clause("Axles", OPERATION.GT)
                        {
                            Label = "gt2Axles",
                            Data = new List<string>()
                            {
                                "2"
                            }
                        }
                    }
                },
                new VehicleRule("Heavy or long")
                {
                    Cost = 10,
                    Severity = 3,
                    Expression = "Weight OR Axles",
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause("Weight", OPERATION.GT)
                        {
                            Label = "Weight",
                            Data = new List<string>()
                            {
                                "4000"
                            }
                        },
                        new Clause("Axles", OPERATION.GT)
                        {
                            Label = "Axles",
                            Data = new List<string>()
                            {
                                "2"
                            }
                        }
                    }
                },
                new VehicleRule("Normal Car"){
                    Cost = 3,
                    Severity = 1,
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause("Weight", OPERATION.GT)
                        {
                            Data = new List<string>()
                            {
                                "1000"
                            }
                        }
                    }
                },
                new VehicleRule("Carpool Car"){
                    Cost = 2,
                    Severity = 2,
                    Target = "Vehicle",
                    Expression = "WeightGT1000 AND WeightLT4000 AND OccupantsGT2",
                    Clauses = new List<Clause>()
                    {
                        new Clause("Weight", OPERATION.GT)
                        {
                            Label = "WeightGT1000",
                            Data = new List<string>()
                            {
                                "1000"
                            }
                        },
                        new Clause("Weight", OPERATION.LT)
                        {
                            Label = "WeightLT4000",
                            Data = new List<string>()
                            {
                                "4000"
                            }
                        },
                        new Clause("Occupants", OPERATION.GT)
                        {
                            Label = "OccupantsGT2",
                            Data = new List<string>()
                            {
                                "2"
                            }
                        },
                    }
                },
                new VehicleRule("Motorcycle"){
                    Cost = 1,
                    Severity = 0,
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause("Weight", OPERATION.LT)
                        {
                            Data = new List<string>()
                            {
                                "1001"
                            }
                        }
                    }
                }
            };
            var analyzer = new Analyzer();
            analyzer.CustomOperationDelegates.Add(OperationDelegate);
            analyzer.CustomOperationValidationDelegates.Add(OperationValidationDelegate);

            var issues = analyzer.EnumerateRuleIssues(rules).ToList();

            Assert.IsFalse(issues.Any());

            Assert.IsTrue(GetCost(overweightTruck, analyzer, rules) == 50);
            Assert.IsTrue(GetCost(truck, analyzer, rules) == 10);// 10
            Assert.IsTrue(GetCost(car, analyzer, rules) == 3); // 3
            Assert.IsTrue(GetCost(carpool, analyzer, rules) == 2); // 2 
            Assert.IsTrue(GetCost(motorcycle, analyzer, rules) == 1); // 1
        }
    }
}
