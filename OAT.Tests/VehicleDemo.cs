using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.CST.OAT.Tests
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

        public (bool Applies, bool Result) OverweightOperationDelegate(Clause clause, IEnumerable<string>? valsToCheck, IEnumerable<KeyValuePair<string, string>> dictToCheck, object? state1, object? state2)
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

        public (bool Applies, IEnumerable<Violation> Violations) OverweightOperationValidationDelegate(Rule r, Clause c)
        {
            if (c.CustomOperation == "OVERWEIGHT")
            {
                var violations = new List<Violation>();
                if (r.Target != "Vehicle")
                {
                    violations.Add(new Violation("Overweight operation requires a Vehicle object", r, c));
                }

                if (c.Data != null || c.DictData != null)
                {
                    violations.Add(new Violation("Overweight operation takes no data.", r, c));
                }
                return (true, violations);
            }
            else
            {
                return (false, Array.Empty<Violation>());
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
                Driver = new Driver() { 
                    License = new DriverLicense() 
                    { 
                        Endorsements = Endorsements.CDL | Endorsements.Auto, 
                        Expiration = DateTime.Now.AddYears(1) 
                    } 
                }
            };

            var overweightTruck = new Vehicle()
            {
                Weight = 30000,
                Capacity = 20000,
                Driver = new Driver() 
                { 
                    License = new DriverLicense() 
                    { 
                        Endorsements = Endorsements.CDL | Endorsements.Auto, 
                        Expiration = DateTime.Now.AddYears(1) 
                    } 
                }
            };

            var expiredLicense = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                Driver = new Driver() 
                { 
                    License = new DriverLicense() 
                    { 
                        Endorsements = Endorsements.CDL | Endorsements.Auto, 
                        Expiration = DateTime.Now.AddYears(-1) 
                    } 
                }
            };

            var noCdl = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                Driver = new Driver() 
                { 
                    License = new DriverLicense() 
                    { 
                        Endorsements = Endorsements.Auto, 
                        Expiration = DateTime.Now.AddYears(1) 
                    } 
                }
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
                        new Clause(OPERATION.CONTAINS, "Driver.License.Endorsements")
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
                        new Clause(OPERATION.IS_EXPIRED, "Driver.License.Expiration")
                        {
                        }
                    }
                }
            };
            var analyzer = new Analyzer();
            analyzer.CustomOperationDelegates.Add(OverweightOperationDelegate);
            analyzer.CustomOperationValidationDelegates.Add(OverweightOperationValidationDelegate);

            var issues = analyzer.EnumerateRuleIssues(rules).ToList();

            Assert.IsFalse(issues.Any());

            Assert.IsTrue(!analyzer.Analyze(rules, truck).Any()); // Compliant
            Assert.IsTrue(analyzer.Analyze(rules, overweightTruck).Any(x => x.Name == "Overweight")); // Overweight
            Assert.IsTrue(analyzer.Analyze(rules, noCdl).Any(x => x.Name == "No CDL")); // Overweight
            Assert.IsTrue(analyzer.Analyze(rules, expiredLicense).Any(x => x.Name == "Expired License")); // Overweight
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
                        new Clause(OPERATION.GT, "Axles")
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
                        new Clause(OPERATION.GT, "Weight")
                        {
                            Label = "Weight",
                            Data = new List<string>()
                            {
                                "4000"
                            }
                        },
                        new Clause(OPERATION.GT, "Axles")
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
                        new Clause(OPERATION.GT, "Weight")
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
                        new Clause(OPERATION.GT, "Weight")
                        {
                            Label = "WeightGT1000",
                            Data = new List<string>()
                            {
                                "1000"
                            }
                        },
                        new Clause(OPERATION.LT, "Weight")
                        {
                            Label = "WeightLT4000",
                            Data = new List<string>()
                            {
                                "4000"
                            }
                        },
                        new Clause(OPERATION.GT, "Occupants")
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
                        new Clause(OPERATION.LT, "Weight")
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
            analyzer.CustomOperationDelegates.Add(OverweightOperationDelegate);
            analyzer.CustomOperationValidationDelegates.Add(OverweightOperationValidationDelegate);

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
