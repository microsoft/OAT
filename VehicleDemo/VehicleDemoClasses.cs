
/* Unmerged change from project 'VehicleDemo (netstandard2.1)'
Before:
using System;
using System.Collections.Generic;
After:
using Microsoft.CST.OAT;
using Microsoft.Collections.Generic;
*/
using Microsoft.CST.OAT.Operations;
using MoreLinq;
using System;
using System.Collections.Generic;

/* Unmerged change from project 'VehicleDemo (netstandard2.1)'
Before:
using Microsoft.CST.OAT;
using Microsoft.CST.OAT.Operations;
using MoreLinq;
After:
using System.Linq;
using System.Text;
using System.Threading.Tasks;
*/
using System.Linq;

namespace Microsoft.CST.OAT.VehicleDemo
{
    public static class VehicleDemoHelpers
    {

        public static int GetCost(Vehicle vehicle, Analyzer analyzer, IEnumerable<Rule> rules)
        {
            // This gets the maximum severity rule that is applied and gets the cost of that rule, if no rules
            // 0 cost
            return ((VehicleRule)analyzer.Analyze(rules, vehicle).MaxBy(x => x.Severity).FirstOrDefault())?.Cost ?? 0;
        }
    }

    public class OverweightOperation : OatOperation
    {
        public OverweightOperation(Analyzer analyzer) : base(Operation.Custom, OverweightOperationDelegate, OverweightOperationValidationDelegate, analyzer, "OverweightOperation")
        {
        }
        public static OperationResult OverweightOperationDelegate(Clause clause, object? state1, object? state2, IEnumerable<ClauseCapture>? captures)
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

        public static IEnumerable<Violation> OverweightOperationValidationDelegate(Rule r, Clause c)
        {
            var violations = new List<Violation>();
            if (r.Target != "Vehicle")
            {
                violations.Add(new Violation("Overweight operation requires a Vehicle object", r, c));
            }

            if (c.Data?.Any() is true || c.DictData.Any() is true)
            {
                violations.Add(new Violation("Overweight operation takes no data.", r, c));
            }
            return violations;
        }
    }

    public class VehicleRule : Rule
    {
        public int Cost;

        public VehicleRule(string name) : base(name)
        {
        }
    }
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
        Truck,
        Bus
    }

    public class Driver
    {
        public Driver()
        {
            License = new DriverLicense();
        }
        public DriverLicense License { get; set; }
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
        public Driver Driver { get; set; } = new Driver();
        public int Occupants { get; set; }
        public VehicleType VehicleType { get; set; }
    }
}
