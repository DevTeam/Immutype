# Pure DI for .NET

[![NuGet](https://buildstats.info/nuget/Immutype)](https://www.nuget.org/packages/Immutype)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[<img src="http://tcavs2015.cloudapp.net/app/rest/builds/buildType:(id:DevTeam_Immutype_BuildAndTestBuildType)/statusIcon"/>](http://tcavs2015.cloudapp.net/viewType.html?buildTypeId=DevTeam_Immutype_BuildAndTestBuildType&guest=1)

_Immutype_ is [.NET code generator](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview). It generates extension methods for records, structures, and classes marked by the attribute *__[Immutype.Target]__* to easily use instances of these types as immutable. _Immutype_ chooses a general constructor for records or a constructor with maximum number of parameters ofr other types.and use it to create a modified clone for an instance using generated static extension methods per each constructor parameter.

For instance, for the type Foo for the constructor parameter *__values__* of type *__IEnumerable<int>__* following extension methods are generated:

| Method | Purpose |
|------- | ------- |
| *__Foo WithValues(this Foo it, params int[] values)__* | replaces values by the new ones using a method with variable number of arguments |
| *__Foo WithValues(this Foo it, IEnumerable<int> values)__* | replaces values by the new ones |
| *__Foo AddValues(this Foo it, params int[] values)__* | adds values using a method with variable number of arguments |
| *__Foo RemoveValues(this Foo it, params int[] values)__* | removes values using a method with variable number of arguments |

For the type Foo for the constructor parameter *__value__* of other types, like *__int__*, with default value *__99__* following extension methods are generated:

| Method | Purpose |
|------- | ------- |
| *__Foo WithValue(this Foo it, int value)__* | replaces a value by the new one |
| *__Foo WithDefaultValue(this Foo it)__* | replaces a value by the default value *__99__* |

The extensions methods above are generating automatically for each *__public__* or *__internal__* type, like *__Foo__* marked by the attribute *__[Immutype.Target]__* in the static class named as *__FooExtensions__*. This generated class *__FooExtensions__* is static, has the same accessibility level and the same namespace like a target class *__Foo__*. Each generated static extension method has two attributes:
- [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] - to improve performance
- [Pure] - to indicates that this method is pure, that is, it does not make any visible state changes

_Immutype_ supports nullable [reference](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references) and [value](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types) types and the following list of enumerable types:

- Arrays like ```T []```
- ```IEnumerable<T>```
- ```List<T>```
- ```List<T>```
- ```IReadOnlyCollection<T>```
- ```IReadOnlyList<T>```
- ```ICollection<T>```
- ```IList<T>```
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

_Immutype_ supports [IIncrementalGenerator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.iincrementalgenerator) as well as [ISourceGenerator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.isourcegenerator) and does not use semantic model, so it works quite effective. 

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

