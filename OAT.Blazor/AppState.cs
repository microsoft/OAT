using Microsoft.CST.OAT.VehicleDemo;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CST.OAT.Blazor
{
    public class AppState
    {
        public List<Assembly> Assemblies { get; set; } = new List<Assembly>();

        public List<Rule> Rules { get; set; } = new List<Rule>();
        public List<object> TestObjects { get; set; } = new List<object>();

        public List<Rule> DemoRules { get; set; } = new List<Rule>()
        {
            new VehicleRule("Overweight Truck by Delegate")
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
            new VehicleRule("Truck")
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
            new VehicleRule("Car"){
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
            },
            new VehicleRule("No CDL")
            {
                Cost = 100,
                Severity = 3,
                Target = "Vehicle",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "VehicleType")
                    {
                        Data = new List<string>(){ "Truck", "Bus" }
                    },
                    new Clause(Operation.Contains, "Driver.License.Endorsements")
                    {
                        Data = new List<string>(){ "CDL" },
                        Invert = true
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
            },
            new VehicleRule("No Auto License"){
                Cost = 75,
                Severity = 1,
                Target = "Vehicle",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "VehicleType")
                    {
                        Data = new List<string>(){ "Car" }
                    },
                    new Clause(Operation.Contains, "Driver.License.Endorsements")
                    {
                        Data = new List<string>(){ "Auto" },
                        Invert = true
                    }
                }
            },
            new VehicleRule("No Motorcycle License"){
                Cost = 75,
                Severity = 1,
                Target = "Vehicle",
                Clauses = new List<Clause>()
                {
                    new Clause(Operation.Equals, "VehicleType")
                    {
                        Data = new List<string>(){ "Motorcycle" }
                    },
                    new Clause(Operation.Contains, "Driver.License.Endorsements")
                    {
                        Data = new List<string>(){ "Motorcycle" },
                        Invert = true
                    }
                }
            }
        };
        public List<Vehicle> DemoTestObjects { get; set; } = new List<Vehicle>()
        {
            new Vehicle()
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
            },
            new Vehicle()
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
            },
            new Vehicle()
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
            },
            new Vehicle()
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
            },
            new Vehicle()
            {
                VehicleType = VehicleType.Car,
                Axles = 2,
                Occupants = 1,
                Driver = new Driver()
                {
                    License = new DriverLicense()
                    {
                        Endorsements = Endorsements.Auto,
                        Expiration = DateTime.Now.AddYears(1)
                    }
                }
            },
            new Vehicle()
            {
                VehicleType = VehicleType.Car,
                Axles = 2,
                Occupants = 3,
                Driver = new Driver()
                {
                    License = new DriverLicense()
                    {
                        Endorsements = Endorsements.Auto,
                        Expiration = DateTime.Now.AddYears(1)
                    }
                }
            },
            new Vehicle()
            {
                VehicleType = VehicleType.Motorcycle,
                Axles = 2,
                Occupants = 1,
                Driver = new Driver()
                {
                    License = new DriverLicense()
                    {
                        Endorsements = Endorsements.Motorcycle,
                        Expiration = DateTime.Now.AddYears(1)
                    }
                }
            }
        };
    }
}
