using Microsoft.CST.OAT.Utils;
using Microsoft.CST.OAT.VehicleDemo;
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
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Logger.SetupVerbose();
            Strings.Setup();
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
                            CustomOperation = "OverweightOperation"
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

            analyzer.SetOperation(new OverweightOperation(analyzer));

            var issues = analyzer.EnumerateRuleIssues(rules).ToList();

            Assert.IsFalse(issues.Any());

            Assert.IsTrue(VehicleDemoHelpers.GetCost(overweightTruck, analyzer, rules) == 50);
            Assert.IsTrue(VehicleDemoHelpers.GetCost(truck, analyzer, rules) == 10);// 10
            Assert.IsTrue(VehicleDemoHelpers.GetCost(car, analyzer, rules) == 3); // 3
            Assert.IsTrue(VehicleDemoHelpers.GetCost(carpool, analyzer, rules) == 2); // 2
            Assert.IsTrue(VehicleDemoHelpers.GetCost(motorcycle, analyzer, rules) == 1); // 1
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
                new VehicleRule("Overweight Script")
                {
                    Cost = 50,
                    Severity = 9,
                    Expression = "Overweight",
                    Target = "Vehicle",
                    Clauses = new List<Clause>()
                    {
                        new Clause(Operation.Script)
                        {
                            Label = "Overweight",
                            Script = new ScriptData(code: @"
if (State1 is Vehicle vehicle)
{
    var res = vehicle.Weight > vehicle.Capacity;
    if ((res && !Clause.Invert) || (Clause.Invert && !res))
    {
        // The rule applies and is true and the capture is available if capture is enabled
        return new OperationResult(true, Clause.Capture ? new TypedClauseCapture<int>(Clause, vehicle.Weight, State1, State2) : null);
    }
}
return new OperationResult(false, null);",
                                imports: new List<string>() {"System", "Microsoft.CST.OAT.VehicleDemo"},
                                references: new List<string>(){ "VehicleDemo" }),
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
            var analyzer = new Analyzer(new AnalyzerOptions(true));

            var issues = analyzer.EnumerateRuleIssues(rules).ToList();

            Assert.IsFalse(issues.Any());

            Assert.IsTrue(!analyzer.Analyze(rules, truck).Any()); // Compliant
            Assert.IsTrue(analyzer.Analyze(rules, overweightTruck).Any(x => x.Name == "Overweight Script")); // Overweight
            Assert.IsTrue(analyzer.Analyze(rules, noCdl).Any(x => x.Name == "No CDL")); // Overweight
            Assert.IsTrue(analyzer.Analyze(rules, expiredLicense).Any(x => x.Name == "Expired License")); // Overweight

            var res = analyzer.GetCaptures(rules, overweightTruck);
            var weight = ((TypedClauseCapture<int>)res.First().Captures[0]).Result;

            Assert.IsTrue(weight == 30000);
        }
    }
}