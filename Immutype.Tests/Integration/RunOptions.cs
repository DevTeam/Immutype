// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Immutype.Tests.Integration;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public class RunOptions
{
    public string Statements = "System.Console.WriteLine(Composer.Resolve<CompositionRoot>().Value);";

    public readonly List<string> AdditionalCode = [];

    public LanguageVersion LanguageVersion = LanguageVersion.Latest;

    public NullableContextOptions NullableContextOptions = NullableContextOptions.Disable;
}