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


## Usage Scenarios

- Basics
  - [Sample scenario](#sample-scenario)
  - [Array](#array)
  - [Applying defaults](#applying-defaults)
  - [Clearing](#clearing)
  - [Immutable collection](#immutable-collection)
  - [Removing](#removing)
  - [Generic types](#generic-types)
  - [Nullable collection](#nullable-collection)
  - [Set](#set)
  - [Record with constructor](#record-with-constructor)
  - [Explicit constructor choice](#explicit-constructor-choice)

### Sample scenario



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    bool HasPassport = true,
    int Age = 0,
    ImmutableArray<Person> Friends = default);

public class SampleScenario
{
    public void Run()
    {
        var john = new Person("John", false, 15)
            .AddFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
            
        john.Friends.Length.ShouldBe(2);

        john = john.WithAge(16).WithDefaultHasPassport();
        john.Age.ShouldBe(16);
        john.HasPassport.ShouldBeTrue();

        john = john.AddFriends(
            new Person("Daniel").WithAge(17),
            new Person("Sophia").WithAge(18));
        
        john.Friends.Length.ShouldBe(4);
            
        john = john.RemoveFriends(new Person("David").WithAge(16));

        john.Friends.Length.ShouldBe(3);
    }
}
```



### Array



``` CSharp
[Immutype.Target]
internal readonly record struct Person(string Name, int Age = 0, params Person[] Friends);

public class Array
{ 
    public void Run()
    {
        var john = new Person("John")
            .WithAge(15)
            .AddFriends(new Person("David").WithAge(16))
            .AddFriends(
                new Person("James"),
                new Person("Daniel").WithAge(17));
        
        john.Friends.Length.ShouldBe(3);
    }
}
```



### Applying defaults



``` CSharp
[Immutype.Target]
internal readonly record struct Person(string Name = "John", int Age = 17);

public class ApplyingDefaults
{
    public void Run()
    {
        var john = new Person("David", 15)
            .WithDefaultAge()
            .WithDefaultName();
        
        john.Name.ShouldBe("John");
        john.Age.ShouldBe(17);
    }
}
```



### Clearing



``` CSharp
[Immutype.Target]
internal readonly record struct Person(
    string Name,
    int Age = 0,
    params Person[] Friends);

public class Clearing
{
    public void Run()
    {
        var john = new Person("John",15, new Person("David").WithAge(16))
            .AddFriends(new Person("James"));

        john = john.ClearFriends();
        
        john.Friends.Length.ShouldBe(0);
    }
}
```



### Immutable collection



``` CSharp
[Immutype.Target]
internal readonly struct Person
{
    public readonly string Name;
    public readonly int Age;
    public readonly IImmutableList<Person> Friends;

    public Person(
        string name,
        int age = 0,
        IImmutableList<Person>? friends = null)
    {
        Name = name;
        Age = age;
        Friends = friends ?? ImmutableList<Person>.Empty;
    }
}

public class ImmutableCollection
{
    public void Run()
    {
        var john = new Person("John",15)
            .WithFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17))
            .AddFriends(
                new Person("David").WithAge(22));
        
        john.Friends.Count.ShouldBe(3);
    }
}
```



### Removing



``` CSharp
[Immutype.Target]
internal readonly record struct Person(
    string Name,
    int Age = 0,
    params Person[] Friends);

public class Removing
{
    public void Run()
    {
        var john = new Person("John",15, new Person("David").WithAge(16))
            .AddFriends(new Person("James"));

        john = john.RemoveFriends(new Person("James"));
        
        john.Friends.Length.ShouldBe(1);
    }
}
```



### Generic types

It is possible to use generic types including any generic constraints.

``` CSharp
[Immutype.Target]
internal record Person<TAge>(string Name, TAge Age = default, IEnumerable<Person<TAge>>? Friends = null) 
    where TAge : struct;

public class GenericTypes
{ 
    public void Run()
    {
        var john = new Person<int>("John")
            .WithAge(15)
            .WithFriends(new Person<int>("David").WithAge(16))
            .AddFriends(
                new Person<int>("James"),
                new Person<int>("Daniel").WithAge(17));
        
        john.Friends?.Count().ShouldBe(3);
    }
}
```



### Nullable collection



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    int? Age = null,
    ICollection<Person>? Friends = null);

public class NullableCollection
{
    public void Run()
    {
        var john = new Person("John",15)
            .AddFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
        
        john.Friends?.Count.ShouldBe(2);
    }
}
```



### Set



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    int Age = 0,
    ISet<Person>? Friends = null);

public class Set
{
    public void Run()
    {
        var john = new Person("John",15)
            .AddFriends(
                new Person("David").WithAge(16),
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
        
        john.Friends?.Count.ShouldBe(2);
    }
}
```



### Record with constructor



``` CSharp
[Immutype.Target]
internal record Person
{
    public Person(
        string name,
        int? age = null,
        ICollection<Person>? friends = null)
    {
        Name = name;
        Age = age;
        Friends = friends;
    }

    public string Name { get; }

    public int? Age { get; }

    public ICollection<Person>? Friends { get; }

    public void Deconstruct(
        out string name,
        out int? age,
        out ICollection<Person>? friends)
    {
        name = Name;
        age = Age;
        friends = Friends;
    }
}

public class RecordWithConstructor
{
    public void Run()
    {
        var john = new Person("John",15)
            .WithFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
        
        john.Friends?.Count.ShouldBe(2);
    }
}
```



### Explicit constructor choice



``` CSharp
[Immutype.Target]
internal readonly struct Person
{
    public readonly string Name;
    public readonly int Age;
    public readonly IImmutableList<Person> Friends;

    // You can explicitly select a constructor by marking it with the [Immutype.Target] attribute
    [Immutype.Target]
    public Person(
        string name,
        int age = 0,
        IImmutableList<Person>? friends = null)
    {
        Name = name;
        Age = age;
        Friends = friends ?? ImmutableList<Person>.Empty;
    }
    
    public Person(
        string name,
        int age,
        IImmutableList<Person>? friends,
        int someArg = 99)
    {
        Name = name;
        Age = age;
        Friends = friends ?? ImmutableList<Person>.Empty;
    }
}

public class ExplicitConstructorChoice
{
    public void Run()
    {
        var john = new Person("John",15)
            .WithFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17))
            .AddFriends(
                new Person("David").WithAge(22));
        
        john.Friends.Count.ShouldBe(3);
    }
}
```

## Built with

<table>
<tr>
<td width="33%" valign="top" align="center">
  <img src="https://raw.githubusercontent.com/DevTeam/dotnet-matrix/afa683e8565b16e24b26b6df277d3244c4d50672/readme/built-with/pure-di.png" width="72" height="72" alt="Pure.DI logo">
  <h3><a href="https://github.com/DevTeam/Pure.DI">Pure.DI</a></h3>
  <p align="left">Wires the source generator's services together at compile time.</p>
  <p><a href="https://www.nuget.org/packages/Pure.DI">NuGet</a></p>
</td>
<td width="33%" valign="top" align="center">
  <img src="https://raw.githubusercontent.com/DevTeam/dotnet-matrix/afa683e8565b16e24b26b6df277d3244c4d50672/readme/built-with/csharp-interactive.png" width="72" height="72" alt="CSharp Interactive logo">
  <h3><a href="https://github.com/DevTeam/csharp-interactive">CSharp Interactive</a></h3>
  <p align="left">Automates builds, tests, and NuGet packaging across Roslyn versions.</p>
  <p><a href="https://www.nuget.org/packages/CSharpInteractive">NuGet</a></p>
</td>
<td width="33%" valign="top" align="center">
  <img src="https://raw.githubusercontent.com/DevTeam/dotnet-matrix/afa683e8565b16e24b26b6df277d3244c4d50672/readme/built-with/ai-client.svg" width="72" height="72" alt="AI Client logo">
  <h3><a href="https://github.com/DevTeam/AI.Client">AI Client</a></h3>
  <p align="left">Supports development with AI chat and MCP tools.</p>
  <p><a href="https://ai.dev-team.org/">Website</a></p>
</td>
</tr>
</table>
