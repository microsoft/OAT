using Microsoft.CST.OAT.Operations;
using Microsoft.CST.OAT.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoreLinq;
using System;
using System.Collections.Generic;
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
            public VehicleType VehicleType { get; internal set; }
        }

        enum VehicleType
        {
            Motorcycle,
            Car,
            Truck
        }

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
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
            Motorcycle = 1,
            Auto = 2,
            CDL = 4
        }

        int GetCost(Vehicle vehicle, Analyzer analyzer, IEnumerable<Rule> rules)
        {
            // This gets the maximum severity rule that is applied and gets the cost of that rule, if no rules 0 cost
            return ((VehicleRule)analyzer.Analyze(rules, vehicle).MaxBy(x => x.Severity).FirstOrDefault())?.Cost ?? 0;
        }

        public OperationResult OverweightOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
        {
            if (state1 is Vehicle vehicle)
            {
                var res = vehicle.Weight > vehicle.Capacity;
                if ((res && !clause.Invert) || (clause.Invert && !res))
                {
                    // The rule applies and is true and the capture is available if capture is enabled
                    return new OperationResult(true, clause.Capture ? new TypedClauseCapture<int>(clause, vehicle.Weight, state1, state2) : null);
                }
            }
            return new OperationResult(false, null);
        }

        public IEnumerable<Violation> OverweightOperationValidationDelegate(Rule r, Clause c)
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
            return violations;
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
                VehicleType = VehicleType.Truck,
                Driver = new Driver()
                {
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
                VehicleType = VehicleType.Truck,
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
                VehicleType = VehicleType.Truck,
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
                VehicleType = VehicleType.Truck,
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
                        new Clause(Operation.Custom)
                        {
                            Label = "Overweight",
                            CustomOperation = "OVERWEIGHT",
                            Capture = true
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
                        new Clause(Operation.Contains, "Driver.License.Endorsements")
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
                        new Clause(Operation.IsExpired, "Driver.License.Expiration")
                        {
                        }
                    }
                }
            };
            var analyzer = new Analyzer();
            var OverweightOperation = new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "OVERWEIGHT",
                OperationDelegate = OverweightOperationDelegate,
                ValidationDelegate = OverweightOperationValidationDelegate
            };

            analyzer.SetOperation(OverweightOperation);

            var issues = analyzer.EnumerateRuleIssues(rules).ToList();

            Assert.IsFalse(issues.Any());

            Assert.IsTrue(!analyzer.Analyze(rules, truck).Any()); // Compliant
            Assert.IsTrue(analyzer.Analyze(rules, overweightTruck).Any(x => x.Name == "Overweight")); // Overweight
            Assert.IsTrue(analyzer.Analyze(rules, noCdl).Any(x => x.Name == "No CDL")); // Overweight
            Assert.IsTrue(analyzer.Analyze(rules, expiredLicense).Any(x => x.Name == "Expired License")); // Overweight

            var res = analyzer.GetCaptures(rules, overweightTruck);
            var weight = ((TypedClauseCapture<int>)res.First().Captures[0]).Result;

            Assert.IsTrue(weight == 30000);
        }

        [TestMethod]
        public void TollBoothDemo()
        {
            var truck = new Vehicle()
            {
                Weight = 20000,
                Capacity = 20000,
                VehicleType = VehicleType.Truck,
                Axles = 5,
                Occupants = 1
            };

            var overweightTruck = new Vehicle()
            {
                Weight = 30000,
                Capacity = 20000,
                VehicleType = VehicleType.Truck,
                Axles = 5,
                Occupants = 1
            };

            var car = new Vehicle()
            {
                VehicleType = VehicleType.Car,
                Axles = 2,
                Occupants = 1
            };

            var carpool = new Vehicle()
            {
                VehicleType = VehicleType.Car,
                Axles = 2,
                Occupants = 3
            };

            var motorcycle = new Vehicle()
            {
                VehicleType = VehicleType.Motorcycle,
                Axles = 2,
                Occupants = 1
            };

            var rules = new VehicleRule[] {
                new VehicleRule("Overweight Truck")
                {
                    Cost = 50,
                    Severity = 9,
                    Expression = "Overweight AND IsTruck",
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause(Operation.Custom)
                        {
                            Label = "Overweight",
                            CustomOperation = "OVERWEIGHT"
                        },
                        new Clause(Operation.Equals, "VehicleType")
                        {
                            Label = "IsTruck",
                            Data = new List<string>()
                            {
                                "Truck"
                            }
                        }
                    }
                },
                new VehicleRule("Regular Truck")
                {
                    Cost = 10,
                    Severity = 5,
                    Expression = "IsTruck",
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause(Operation.Equals, "VehicleType")
                        {
                            Label = "IsTruck",
                            Data = new List<string>()
                            {
                                "Truck"
                            }
                        }
                    }
                },
                new VehicleRule("Car with Trailer")
                {
                    Cost = 10,
                    Severity = 3,
                    Expression = "IsCar AND Axles",
                    Target = "Vehicle",
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
                        new Clause(Operation.GreaterThan, "Axles")
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
                        new Clause(Operation.Equals, "VehicleType")
                        {
                            Data = new List<string>()
                            {
                                "Car"
                            }
                        }
                    }
                },
                new VehicleRule("Carpool Car"){
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
                },
                new VehicleRule("Motorcycle"){
                    Cost = 1,
                    Severity = 0,
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause(Operation.Equals, "VehicleType")
                        {
                            Data = new List<string>()
                            {
                                "Motorcycle"
                            }
                        }
                    }
                }
            };
            var analyzer = new Analyzer();
            var OverweightOperation = new OatOperation(Operation.Custom, analyzer)
            {
                CustomOperation = "OVERWEIGHT",
                OperationDelegate = OverweightOperationDelegate,
                ValidationDelegate = OverweightOperationValidationDelegate
            };

            analyzer.SetOperation(OverweightOperation);

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
