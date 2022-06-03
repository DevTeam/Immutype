# Immutype

[![NuGet](https://buildstats.info/nuget/Immutype)](https://www.nuget.org/packages/Immutype)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[<img src="http://teamcity.jetbrains.com/app/rest/builds/buildType:(id:OpenSourceProjects_DevTeam_Immutype_BuildAndTestBuildType)/statusIcon"/>](http://teamcity.jetbrains.com/viewType.html?buildTypeId=OpenSourceProjects_DevTeam_Immutype_BuildAndTestBuildType&guest=1)

_Immutype_ is [.NET code generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) creating extension methods for records, structures, and classes marked by the attribute `````[Immutype.Target]````` to efficiently operate with instances of these types like with immutable ones.

For instance, for the type Foo for the constructor parameter *__values__* of type ```IEnumerable<int>``` following extension methods are generated:

| Method | Purpose |
|------- | ------- |
| ```Foo WithValues(this Foo it, params int[] values)``` | replaces values by the new ones using a method with variable number of arguments |
| ```Foo WithValues(this Foo it, IEnumerable<int> values)``` | replaces values by the new ones |
| ```Foo AddValues(this Foo it, params int[] values)``` | adds values using a method with variable number of arguments |
| ```Foo AddValues(this Foo it, IEnumerable<int> values)``` | adds values |
| ```Foo RemoveValues(this Foo it, params int[] values)``` | removes values using a method with variable number of arguments |
| ```Foo RemoveValues(this Foo it, IEnumerable<int> values)``` | removes values |

For the type Foo for the constructor parameter *__value__* of other types, like ```int```, with default value ```99``` following extension methods are generated:

| Method | Purpose |
|------- | ------- |
| ```Foo WithValue(this Foo it, int value)``` | replaces a value by the new one |
| ```Foo WithDefaultValue(this Foo it)``` | replaces a value by the default value *__99__* |

The extensions methods above are generating automatically for each ```public``` or ```internal``` type, like *__Foo__* marked by the attribute ```[Immutype.Target]``` in the static class named as *__FooExtensions__*. This generated class *__FooExtensions__* is static, has the same accessibility level and the same namespace like a target class *__Foo__*. Each generated static extension method has two attributes:
- ```[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]``` - to improve performance
- ```[Pure]``` - to indicates that this method is pure, that is, it does not make any visible state changes

_Immutype_ supports nullable [reference](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references) and [value](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types) types and the following list of enumerable types:

- Arrays
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
- ```IReadOnlyCollection<T>```
- ```IReadOnlyList<T>```
- ```IReadOnlySet<T>```
- ```ImmutableList<T>```
- ```IImmutableList<T>```
- ```ImmutableArray<T>```
- ```ImmutableQueue<T>```
- ```IImmutableQueue<T>```
- ```ImmutableStack<T>```
- ```IImmutableStack<T>```

_Immutype_ supports [IIncrementalGenerator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.iincrementalgenerator) as well as [ISourceGenerator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.isourcegenerator) so it works quite effective.

## NuGet package

[![NuGet](https://buildstats.info/nuget/Immutype)](https://www.nuget.org/packages/Immutype)

- Package Manager

  ```
  Install-Package Immutype
  ```

- .NET CLI

  ```
  dotnet add package Immutype
  ```

## Development environment requirements

- [.NET SDK 5.0.102+](https://dotnet.microsoft.com/download/dotnet/5.0)
- [C# v.6 or newer](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-40)

## Supported frameworks

- [.NET and .NET Core](https://docs.microsoft.com/en-us/dotnet/core/) 1.0+
- [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) 1.0+
- [.NET Framework](https://docs.microsoft.com/en-us/dotnet/framework/) 3.5+
- [UWP/XBOX](https://docs.microsoft.com/en-us/windows/uwp/index)
- [.NET IoT](https://dotnet.microsoft.com/apps/iot)
- [Xamarin](https://dotnet.microsoft.com/apps/xamarin)
- [.NET Multi-platform App UI (MAUI)](https://docs.microsoft.com/en-us/dotnet/maui/)

