# Logical Analyzer

Logical Analyzer is a rules processing engine to apply provided rules against arbitrary objects

* [Rules](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Rule.cs) contain a Target, a Severity, a boolean Expression and a List of [Clauses](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Clause.cs) which are applied to the targeted object.
* [Clauses](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Clause.cs) perform a specified [Operation](https://github.com/microsoft/LogicalAnalyzer/blob/main/LogicalAnalyzer/Operation.cs) on a specified Field of a Target.  The Field can be any property or subproperty or field of the object specified with dot notation to separate levels. 
* The Analyzer has 4 delegate extensibility points.
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
## Authoring Rules

Detailed information on how to author rules is available on on the [wiki](https://github.com/microsoft/LogicalAnalyzer/wiki/Authoring-Rules).

## Detailed Usage

A full walkthrough is available in the [wiki](https://github.com/microsoft/LogicalAnalyzer/wiki/Walkthrough).

## Delegates

Documentation for implementing each delegate is available in the [wiki](https://github.com/microsoft/LogicalAnalyzer/wiki/Delegates).

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
