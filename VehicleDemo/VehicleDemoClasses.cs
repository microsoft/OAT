using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CST.OAT.VehicleDemo
{
    [Flags]
    public enum Endorsements
    {
        Motorcycle = 1,
        Auto = 2,
        CDL = 4
    }

    public enum VehicleType
    {
        Motorcycle,
        Car,
        Truck
    }

    public class Driver
    {
        public DriverLicense? License { get; set; }
    }

    public class DriverLicense
    {
        public Endorsements Endorsements { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class Vehicle
    {
        public string Plate { get; set; } = string.Empty;
        public int Weight;
        public int Axles { get; set; }
        public int Capacity { get; set; }
        public Driver? Driver { get; set; }
        public int Occupants { get; set; }
        public VehicleType VehicleType { get; set; }
    }
}
