# Introduction

Logical Analyzer is a rules processing engine to apply arbitrary provided rules against arbitrary objects

* LogicalAnalyzer is a powerful rules processing engine.
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

## Usage

The basic usage of Logical Analyzer is applying rules to targets using the Analyze function.

```csharp
Object target;
List<Rule> rules;
var analyzer = new Analyzer();
var rulesWhichApply = analyzer.Analyze(rules,target);
```

## Example

This is an example with objects and rules created for those objects.

```csharp
class Person
{
    string FirstName;
    string SecondName;
    DateTime BirthDate;
}

var professorx = new Person()
{
    FirstName = "Charles",
    SecondName = "Xavier",
    BirthDate = Convert.ToDateTime("1/1/1932")
}

var magneto = new Person()
{
    FirstName = "Max",
    SecondName = "Eisenhardt",
    BirthDate = Convert.ToDateTime("1/1/1930")
}

var rules = new Rule[] {
    new Rule(){
        Expression = "(Name AND Age)",
        Target = "Person",
        Clauses = new List<Clause>()
        {
            new Clause("FirstName", OPERATION.EQ)
            {
                Label = "Name",
                Data = new List<string>()
                {
                    "Charles"
                }
            },
            new Clause("Age", OPERATION.IS_BEFORE)
            {
                Label = "Age",
                Data = new List<string>()
                {
                    DateTime.Now.AddYears(-18).ToString();
                }
            }
        }
    }
}

var analyzer = new Analyzer();
var results = analyzer.Analyze(rules,professorx); // And Rule
results = analyzer.Analyze(rules,magneto); // Empty set
```

## Delegate Extensibility

### Property Parsing
In Attack Surface Analyzer (ASA) we extend property parsing to support our usage of TpmAlgId in dictionaries.
https://github.com/microsoft/AttackSurfaceAnalyzer/blob/e68ab71c6d36403f354e23b969669ad7169b48d7/Lib/Utils/AsaAnalyzer.cs#L23-L41

### Value Extraction
In Asa we also extend Value extraction for the same reason.
https://github.com/microsoft/AttackSurfaceAnalyzer/blob/e68ab71c6d36403f354e23b969669ad7169b48d7/Lib/Utils/AsaAnalyzer.cs#L43-L50

And then we specify the delegates for the Analyzer.
https://github.com/microsoft/AttackSurfaceAnalyzer/blob/e68ab71c6d36403f354e23b969669ad7169b48d7/Lib/Utils/AsaAnalyzer.cs#L51-L55

### Custom Operation
In the tests we test extending with a custom Operation:
https://github.com/microsoft/LogicalAnalyzer/blob/05ddbb9c21a37b1c9c0889bbd5f230ca61673489/LogicalAnalyzer.Tests/ExpressionsTests.cs#L581-L591

### Rule Validation
We also test extending the validation logic with a delegate:
https://github.com/microsoft/LogicalAnalyzer/blob/05ddbb9c21a37b1c9c0889bbd5f230ca61673489/LogicalAnalyzer.Tests/ExpressionsTests.cs#L901-L917

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
