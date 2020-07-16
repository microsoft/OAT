# Logical Analyzer

Logical Analyzer is a rules processing engine to apply arbitrary provided rules against arbitrary objects

* Rules contain an arbitrary boolean expression of the results of Clauses.
* [Clauses](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Clause.cs) perform a specified [Operation](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Operation.cs) on a specified Field of a Target.  The Field can be any property or subproperty specified with dot notation to separate levels. 
* When Field is null the root object is used. 
* The Analyzer now can parse any kind of Object, the specific Object Type a rule applies to is determined by the Target field in the Rule.   For example, AttackSurfaceAnalyzer we extend Rule with some Fields in [AsaRule](https://github.com/microsoft/AttackSurfaceAnalyzer/pull/516/files#diff-0b512edc1f875025bdc9925a7ad29713) and set the Target based on the ResultType set in the Rule.
* Target can also be null as a wildcard.
* The Analyzer has 4 delegate extensibility points, with code examples below.
1. Property Parsing
2. Value Extraction
3. Custom Operation
4. Rule Validation

## Basic Usage

The basic usage of Logical Analyzer is applying rules to targets using the Analyze function.

```csharp
object target;
IEnumerable<Rule> rules;
var analyzer = new Analyzer();
var rulesWhichApply = analyzer.Analyze(rules,target);
```

## Detailed Usage

This detailed example shows using Logical Analyzer to make a car tolling system.

```csharp
class Vehicle
{
    public int Weight { get; set; }
    public int Axles { get; set; }
    public int Occupants { get; set; }
}

int GetCost(Vehicle vehicle, Analyzer analyzer, IEnumerable<Rule> rules)
{
    return ((VehicleRule)analyzer.Analyze(rules, vehicle).MaxBy(x => x.Severity).First()).Cost;
}
public class VehicleRule : Rule
{
    public int Cost;
    public VehicleRule(string name) : base(name) { }
}

var rules = new VehicleRule[] {
    new VehicleRule("Overweight or long")
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

[TestMethod]
public void TestVehicleDemo()
{
    var truck = new Vehicle()
    {
        Weight = 20000,
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

    var analyzer = new Analyzer();

    Assert.IsTrue(GetCost(truck, analyzer, rules) == 10);// 10
    Assert.IsTrue(GetCost(car, analyzer, rules) == 3); // 3
    Assert.IsTrue(GetCost(carpool, analyzer, rules) == 2); // 2 
    Assert.IsTrue(GetCost(motorcycle, analyzer, rules) == 1); // 1
}
```

## Delegates

Logical Analyzer has 4 delegate extensibility points, examples of using each are below.

### Property Parsing
In Attack Surface Analyzer (ASA) we extend Logical Analyzer property parsing to support our usage of TpmAlgId in dictionaries.
```csharp
public static (bool, object?) ParseCustomAsaProperties(object? obj, string index)
{
    switch (obj)
    {
        case Dictionary<(TpmAlgId, uint), byte[]> algDict:
            var elements = Convert.ToString(index, CultureInfo.InvariantCulture)?.Trim('(').Trim(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (Enum.TryParse(typeof(TpmAlgId), elements.First(), out object? result) &&
                result is TpmAlgId Algorithm && uint.TryParse(elements.Last(), out uint Index) &&
                algDict.TryGetValue((Algorithm, Index), out byte[]? byteArray))
            {
                return (true, byteArray);
            }
            else
            {
                return (true, null);
            }
    }
    return (false, null);
}
```

### Value Extraction
In Asa we also extend Value extraction for the same reason.
```csharp
public static (bool Processed, IEnumerable<string> valsExtracted, IEnumerable<KeyValuePair<string, string>> dictExtracted) ParseCustomAsaObjectValues(object? obj)
{
    if (obj is Dictionary<(TpmAlgId, uint), byte[]> algDict)
    {
        return (true,Array.Empty<string>(), algDict.ToList().Select(x => new KeyValuePair<string, string>(x.Key.ToString(), Convert.ToBase64String(x.Value))).ToList());
    }
    return (false, Array.Empty<string>(), Array.Empty<KeyValuePair<string,string>>());
}
```

### Custom Operation
In the tests we test extending with a custom Operation:
```csharp
var analyzer = new Analyzer();

analyzer.CustomOperationDelegate = fooOperation;

bool fooOperation(clause, listValues, dictionaryValues) =>
{
    if (clause.Operation == OPERATION.CUSTOM)
    {
        if (clause.CustomOperation == "FOO")
        {
            return true;
        }
    }
    return false;
};
```

### Rule Validation
We also test extending the validation logic with a delegate:
```csharp
analyzer.CustomOperationValidationDelegate = parseFooOperations;

IEnumerable<Violation> parseFooOperations(Rule r, Clause c)
{
    switch (c.CustomOperation)
    {
        case "FOO":
            if (!c.Data.Any())
            {
                yield return new Violation("FOO Operation expects data", r, c);
            }
            break;
        default:
            yield return new Violation($"{c.CustomOperation} is unexpected", r, c);
            break;
    }
};
```

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
