# Object Analysis Toolkit

Object Analysis Toolkit (OAT) is a rules processing engine to apply provided rules against arbitrary objects.

OAT is available on NuGet as [Microsoft.CST.OAT](https://www.nuget.org/packages/Microsoft.CST.OAT/)

## Overview
* [Rules](https://github.com/microsoft/OAT/blob/main/OAT/Rule.cs) contain a Target, a Severity, a boolean Expression and a List of [Clauses](https://github.com/microsoft/OAT/blob/main/OAT/Clause.cs) which are applied to the targeted object.
* [Clauses](https://github.com/microsoft/OAT/blob/main/OAT/Clause.cs) perform a specified [Operation](https://github.com/microsoft/OAT/blob/main/OAT/Operation.cs) on a specified Field of a Target.  The Field can be any property or subproperty or field of the object specified with dot notation to separate levels.
* Clauses can also Capture the result of their operation.
* The Operation set can be expanded with delegates.
* The object types supported by existing operations can also be expanded with delegates.

Check the [Wiki](https://github.com/microsoft/OAT/wiki) for additional detail.

## Basic Usage

The basic usage of OAT is applying rules to targets using the Analyze function.

```csharp
object target;
IEnumerable<Rule> rules;
var analyzer = new Analyzer();
var rulesWhichApply = analyzer.Analyze(rules,target);
```

## Capturing

OAT also supports capturing and returning results of clauses.

```csharp
object target;
IEnumerable<Rule> rules;
var analyzer = new Analyzer();
var res = analyzer.GetCaptures(rules, target);
```

## Detailed Usage

A full [walkthrough](https://github.com/microsoft/OAT/wiki/Walkthrough) includng creating a custom operation and validating your custom operation rules and capturing Clause values is available on the wiki.

## Authoring Rules

Detailed information on how to [author rules](https://github.com/microsoft/OAT/wiki/Authoring-Rules) is available on on the wiki.

## Delegates

Documentation for implementing each [delegate](https://github.com/microsoft/OAT/wiki/Delegates) is available on the wiki.

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
