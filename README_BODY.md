<img src="Docs/Images/icon.png" width="72" height="72" alt="Immutype logo">

# Immutype

[![NuGet](https://img.shields.io/nuget/v/Immutype)](https://www.nuget.org/packages/Immutype)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[<img src="http://teamcity.jetbrains.com/app/rest/builds/buildType:(id:OpenSourceProjects_DevTeam_Immutype_BuildAndTestBuildType)/statusIcon"/>](http://teamcity.jetbrains.com/viewType.html?buildTypeId=OpenSourceProjects_DevTeam_Immutype_BuildAndTestBuildType&guest=1)
![GitHub Build](https://github.com/DevTeam/Immutype/actions/workflows/main.yml/badge.svg)

Immutype is a C# source generator that adds extension methods to records, structs, and classes marked with `[Immutype.Target]`. These methods return an updated instance and leave the original instance untouched.

## Quick start

In a C# console project, install the [Immutype NuGet package](https://www.nuget.org/packages/Immutype).

```sh
dotnet add package Immutype
```

Replace `Program.cs` with this example (C# 9 or later) and run `dotnet run`.

```csharp
using System;

var alice = new Person("Alice");
var updated = alice.AddFriends(new Person("Bob"));

Console.WriteLine(alice.Friends.Length);    // 0
Console.WriteLine(updated.Friends[0].Name); // Bob

[Immutype.Target]
internal record Person(string Name, params Person[] Friends);
```

In Visual Studio's Package Manager Console, use `Install-Package Immutype`.

## Generated methods

For instance, for the type Foo for the constructor parameter *__values__* of type ```IEnumerable<int>``` following extension methods are generated:

- ```Foo WithValues(this Foo it, params int[] values)``` - to replace values by the new ones using a method with variable number of arguments
- ```Foo WithValues(this Foo it, IEnumerable<int> values)``` - to replace values by the new ones
- ```Foo AddValues(this Foo it, params int[] values)``` - to add values using a method with variable number of arguments
- ```Foo AddValues(this Foo it, IEnumerable<int> values)``` - to add values
- ```Foo RemoveValues(this Foo it, params int[] values)``` - to remove values using a method with variable number of arguments
- ```Foo RemoveValues(this Foo it, IEnumerable<int> values)``` - to remove values
- ```Foo ClearValues(this Foo it)``` - to clear all values

For the type Foo for the constructor parameter *__value__* of other types, like ```int```, with default value ```99``` following extension methods are generated:

- ```Foo WithValue(this Foo it, int value)``` - to replace a value by the new one
- ```Foo WithDefaultValue(this Foo it)``` - to replace a value by the default value *__99__*

The extensions methods above are generating automatically for each ```public``` or ```internal``` type, like *__Foo__* marked by the attribute ```[Immutype.Target]``` in the static class named as *__FooExtensions__*. This generated class *__FooExtensions__* is static, has the same accessibility level and the same namespace like a target class *__Foo__*. Each generated static extension method has two attributes:
- ```[MethodImpl(MethodImplOptions.AggressiveInlining)]``` - to encourage inlining
- ```[Pure]``` - to indicate that this method is pure, that is, it does not make any visible state changes

Immutype supports nullable reference and value types. It generates collection operations for these types.

- Arrays of the form `T[]`
- ```IEnumerable<T>```
- ```List<T>```
- ```IList<T>```
- ```IReadOnlyCollection<T>```
- ```IReadOnlyList<T>```
- ```ICollection<T>```
- ```HashSet<T>```
- ```ISet<T>```
- ```Queue<T>```
- ```Stack<T>```
- ```ImmutableList<T>```
- ```IImmutableList<T>```
- ```ImmutableArray<T>```
- ```ImmutableQueue<T>```
- ```IImmutableQueue<T>```
- ```ImmutableStack<T>```
- ```IImmutableStack<T>```

## Compatibility

### Using the NuGet package

The published package requires Roslyn 4.3 or later. Use .NET SDK 6.0.400 or later for command-line builds, or Visual Studio 2022 17.3 or later. See the [Roslyn toolset version mapping](https://github.com/dotnet/roslyn/blob/main/docs/wiki/NuGet-packages.md#versioning).

Basic class and struct scenarios work with C# 6. Nullable reference annotations require C# 8, records require C# 9, and record structs require C# 10.

Immutype runs at build time and adds no runtime dependency on the generator. Your target framework needs to provide the APIs used by the generated code, including LINQ for collection operations. Add a `System.Collections.Immutable` reference for immutable collections if your target framework requires it.

### Building Immutype from source

Install the .NET 11 SDK specified in [global.json](global.json). Run the tests from the repository root.

```sh
dotnet test Immutype.sln
```

