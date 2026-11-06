# Object Analysis Toolkit
![Nuget](https://img.shields.io/nuget/v/Microsoft.CST.OAT?link=https://www.nuget.org/packages/Microsoft.CST.OAT/&link=https://www.nuget.org/packages/Microsoft.CST.OAT/) ![Nuget](https://img.shields.io/nuget/dt/Microsoft.CST.OAT?link=https://www.nuget.org/packages/Microsoft.CST.OAT/&link=https://www.nuget.org/packages/Microsoft.CST.OAT/)

Object Analysis Toolkit (OAT) is a rules driven metaprogramming engine for arbitrary C# objects.

## Getting OAT
OAT is available on NuGet as [Microsoft.CST.OAT](https://www.nuget.org/packages/Microsoft.CST.OAT/).

## Documentation
Docuemtnation for usage is on the [Wiki](https://github.com/microsoft/OAT/wiki).

API documentation can be generated locally from the repository root using `docfx docfx/docfx.json`; it is no longer automatically published to GitHub Pages.

## Overview
* [Rules](xref:Microsoft.CST.OAT.Rule) contain a object Target, an int Severity, a string boolean Expression and a List of [Clauses](xref:Microsoft.CST.OAT.Clause) which are applied to the targeted object.
* [Clauses](xref:Microsoft.CST.OAT.Clause) perform a specified [Operation](xref:Microsoft.CST.OAT.Operation) on a specified Field of the Target.  The Field can be any property or subproperty or field of the object. For example, `SomeFieldOfTarget.SomeObject.SomeProperty`.
* Clauses can also Capture the result of their operation, to be returned with the applied Rule.
* The Operation set can be expanded with delegates.
* The object types supported by existing operations can also be expanded with delegates.

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

A full [walkthrough](https://github.com/microsoft/OAT/wiki/Walkthrough) including creating a custom operation and validating your custom operation rules and capturing Clause values is available on the wiki.

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