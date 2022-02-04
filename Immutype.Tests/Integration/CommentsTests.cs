

// ReSharper disable StringLiteralTypo

namespace Immutype.Tests.Integration;

public class CommentsTests
{
    [Fact]
    public void ShouldGenerateCommentWhenEnumerable()
    {
        // Given
        const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).WithValues(99, 66).values));";

        // When
        var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(
                    // AbcComment
                    IEnumerable<int> values);
            }".Run(out var generatedCode, new RunOptions
        {
            Statements = statements
        });

        // Then
        output.ShouldBe(new[]
        {
            "99,66"
        }, generatedCode);
        
        generatedCode.Contains("/// Set <c>Values</c>. AbcComment").ShouldBeTrue();
        generatedCode.Contains("/// <param name=\"it\">The original instance.</param>").ShouldBeTrue();
        generatedCode.Contains("/// <param name=\"values\"><c>Values</c> to be changed in the copy of the instance.</param>").ShouldBeTrue();
        generatedCode.Contains("/// <returns>The modified copy of the original instance.</returns>").ShouldBeTrue();
    }
    
    [Fact]
    public void ShouldGenerateComment()
    {
        // Given
        const string statements = "System.Console.WriteLine(new Rec(33).WithVal(99));";

        // When
        var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.TargetAttribute()]
                public record Rec(
                    // AbcComment
                    int val);
            }".Run(out var generatedCode, new RunOptions
        {
            Statements = statements
        });

        // Then
        output.ShouldBe(new[]
        {
            "Rec { val = 99 }"
        }, generatedCode);
        
        generatedCode.Contains("/// Set <c>Val</c>. AbcComment").ShouldBeTrue();
        generatedCode.Contains("<param name=\"it\">The original instance.</param>").ShouldBeTrue();
        generatedCode.Contains("<param name=\"val\"><c>Val</c> to be changed in the copy of the instance.</param>").ShouldBeTrue();
        generatedCode.Contains("<returns>The modified copy of the original instance.</returns>").ShouldBeTrue();
    }
    
    [Fact]
    public void ShouldGenerateCommentWhenSeveralLines()
    {
        // Given
        const string statements = "System.Console.WriteLine(new Rec(33).WithVal(99));";

        // When
        var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.TargetAttribute()]
                public record Rec(
                    // AbcComment
                    // Xyz
                    int val);
            }".Run(out var generatedCode, new RunOptions
        {
            Statements = statements
        });

        // Then
        output.ShouldBe(new[]
        {
            "Rec { val = 99 }"
        }, generatedCode);
        
        generatedCode.Contains("/// Set <c>Val</c>. AbcComment").ShouldBeTrue();
        generatedCode.Contains("/// Xyz").ShouldBeTrue();
        generatedCode.Contains("<param name=\"it\">The original instance.</param>").ShouldBeTrue();
        generatedCode.Contains("<param name=\"val\"><c>Val</c> to be changed in the copy of the instance.</param>").ShouldBeTrue();
        generatedCode.Contains("<returns>The modified copy of the original instance.</returns>").ShouldBeTrue();
    }
}