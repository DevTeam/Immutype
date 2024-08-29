

// ReSharper disable StringLiteralTypo

namespace Immutype.Tests.Integration;

public class GenericTypeTests
{
    [Fact]
    public void ShouldSupportGenerics()
    {
        // Given
        const string statements = "System.Console.WriteLine(string.Join(',', new Rec<int, string>(new List<int>{33}).WithVals(55).AddVals(99, 44).vals));";

        // When
        var output = """
                     
                                 namespace Sample
                                 {
                                     using System;
                                     using System.Collections.Generic;
                                     using Immutype;
                     
                                     [Target]
                                     public record Rec<T, T2>(IList<T> vals);
                                 }
                     """.Run(out var generatedCode, new RunOptions
        {
            Statements = statements
        });

        // Then
        output.ShouldBe([
            "55,99,44"
        ], generatedCode);
    }

    [Fact]
    public void ShouldSupportGenericConstraints()
    {
        // Given
        const string statements = "System.Console.WriteLine(string.Join(',', new Rec<int, string>(new List<int>{33}).WithVals(55).AddVals(99, 44).vals));";

        // When
        var output = """
                     
                                 namespace Sample
                                 {
                                     using System;
                                     using System.Collections.Generic;
                                     using Immutype;
                     
                                     [Target]
                                     public record Rec<T, T2>(IList<T> vals)
                                         where T: struct;
                                 }
                     """.Run(out var generatedCode, new RunOptions
        {
            Statements = statements
        });

        // Then
        output.ShouldBe([
            "55,99,44"
        ], generatedCode);
    }
}