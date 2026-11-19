namespace Immutype.Tests.Integration;

using Microsoft.CodeAnalysis;

public class GenerationTests
{
    [Fact]
    public void ShouldResolveAliasesAndDefaultsInNestedNamespaceScopes()
    {
        var output = """
                     using Number = System.Int32;

                     namespace Sample
                     {
                         using static Defaults;

                         public static class Defaults
                         {
                             public const int Initial = 7;
                         }

                         namespace Models
                         {
                             using Text = System.String;

                             [Immutype.Target]
                             public record Item(Text Name, Number Value = Initial);
                         }
                     }
                     """.Run(out var generatedCode, new RunOptions
        {
            AdditionalCode = { "global using Sample.Models;" },
            Statements = """
                         var item = new Item("original", 3);
                         var copy = item.WithName("updated").WithDefaultValue();
                         Console.WriteLine($"{copy.Name}:{copy.Value}:{item.Name}:{item.Value}");
                         """
        });

        output.ShouldBe(["updated:7:original:3"], generatedCode);
    }

    [Fact]
    public void ShouldReportTheNullReceiverOrArgument()
    {
        var output = """
                     namespace Sample
                     {
                         [Immutype.Target]
                         public record Item(string Name, int[] Values);
                     }
                     """.Run(out var generatedCode, new RunOptions
        {
            Statements = """
                         var item = new Item("original", new[] { 1 });
                         foreach (var operation in new Action[]
                         {
                             () => ((Item)null).WithName("updated"),
                             () => item.WithName(null),
                             () => item.WithValues((int[])null),
                             () => item.AddValues((int[])null),
                             () => item.RemoveValues((int[])null),
                             () => ((Item)null).ClearValues()
                         })
                         {
                             try { operation(); }
                             catch (ArgumentNullException error) { Console.WriteLine(error.ParamName); }
                         }
                         """
        });

        output.ShouldBe(["it", "Name", "Values", "Values", "Values", "it"], generatedCode);
    }

    [Fact]
    public void ShouldTreatNullCollectionsAsEmptyAndPreserveExceptSemantics()
    {
        var output = """
                     using System.Collections.Generic;

                     namespace Sample
                     {
                         [Immutype.Target]
                         public record Item(string Name, List<int>? Values);
                     }
                     """.Run(out var generatedCode, new RunOptions
        {
            NullableContextOptions = NullableContextOptions.Enable,
            Statements = """
                         var original = new Item("original", null);
                         var added = original.AddValues(3, 1, 3, 2, 2);
                         var copied = added.AddValues((List<int>?)null);
                         var removed = copied.RemoveValues(1);
                         var distinct = copied.RemoveValues((List<int>?)null);
                         Console.WriteLine(original.Values is null);
                         Console.WriteLine(string.Join(",", original.RemoveValues(1).Values!));
                         Console.WriteLine(string.Join(",", added.Values!));
                         Console.WriteLine(string.Join(",", removed.Values!));
                         Console.WriteLine(string.Join(",", distinct.Values!));
                         Console.WriteLine(removed.Name);
                         """
        });

        output.ShouldBe(["True", "", "3,1,3,2,2", "3,2", "3,1,2", "original"], generatedCode);
    }

    [Fact]
    public void ShouldReadTheSourceCollectionOncePerOperation()
    {
        var output = """
                     using System.Collections.Generic;

                     namespace Sample
                     {
                         [Immutype.Target]
                         public class Item
                         {
                             private readonly List<int> _values;

                             public Item(List<int> values) => _values = values;

                             public int Reads { get; private set; }

                             public List<int> Values
                             {
                                 get
                                 {
                                     Reads++;
                                     return _values;
                                 }
                             }
                         }
                     }
                     """.Run(out var generatedCode, new RunOptions
        {
            Statements = """
                         var item = new Item(new List<int> { 1, 2 });
                         Console.WriteLine(string.Join(",", item.AddValues(3).Values));
                         Console.WriteLine(string.Join(",", item.RemoveValues(1).Values));
                         Console.WriteLine(item.Reads);
                         """
        });

        output.ShouldBe(["1,2,3", "2", "2"], generatedCode);
    }
}
