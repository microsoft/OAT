# Logical Analyzer

Logical Analyzer is a rules processing engine to apply provided rules against arbitrary objects

* [Rules](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Rule.cs) contain a Target, a Severity, a boolean Expression and a List of [Clauses](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Clause.cs) which are applied to the targeted object.
* [Clauses](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Clause.cs) perform a specified [Operation](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Operation.cs) on a specified Field of a Target.  The Field can be any property or subproperty or field of the object specified with dot notation to separate levels. 
* The Analyzer has 4 delegate extensibility points, with code examples below.
1. Property Parsing
2. Value Extraction
3. Custom Operation
4. Rule Validation

Check the [Wiki](https://github.com/microsoft/LogicalAnalyzer/wiki) for additional detail.

## Basic Usage

The basic usage of Logical Analyzer is applying rules to targets using the Analyze function.

```csharp
object target;
IEnumerable<Rule> rules;
var analyzer = new Analyzer();
var rulesWhichApply = analyzer.Analyze(rules,target);
```

## Detailed Usage

This detailed example shows using Logical Analyzer for a car tolling system.  The example with the rules used is available as a runnable demo in the [Tests](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer.Tests/VehicleDemo.cs).

```csharp
class Vehicle
{
    public int Weight;
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

    Assert.IsTrue(GetCost(truck, analyzer, rules) == 10);
    Assert.IsTrue(GetCost(car, analyzer, rules) == 3);
    Assert.IsTrue(GetCost(carpool, analyzer, rules) == 2);
    Assert.IsTrue(GetCost(motorcycle, analyzer, rules) == 1);
}
```

## Delegates

Logical Analyzer has 4 delegate extensibility points, examples of using each are below.

### Property Parsing
In [Attack Surface Analyzer](https://github.com/microsoft/AttackSurfaceAnalyzer/) we extend Logical Analyzer property parsing to support our usage of TpmAlgId in dictionaries. [link](https://github.com/microsoft/AttackSurfaceAnalyzer/blob/ed94a33f6b3c9884bda995e1e03c5ac533e3f559/Lib/Utils/AsaAnalyzer.cs#L23)
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
In [Attack Surface Analyzer](https://github.com/microsoft/AttackSurfaceAnalyzer/) we also extend Value extraction for the same reason. [link](https://github.com/microsoft/AttackSurfaceAnalyzer/blob/ed94a33f6b3c9884bda995e1e03c5ac533e3f559/Lib/Utils/AsaAnalyzer.cs#L43)
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
In the tests we test extending with a custom Operation [link](https://github.com/microsoft/LogicalAnalyzer/blob/df5407784ff2c39bc79bb0d1dc03b760c97598a1/LogicalAnalyzer.Tests/ExpressionsTests.cs#L581)
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
We also test extending the validation logic with a delegate [link](https://github.com/microsoft/LogicalAnalyzer/blob/df5407784ff2c39bc79bb0d1dc03b760c97598a1/LogicalAnalyzer.Tests/ExpressionsTests.cs#L903)
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
