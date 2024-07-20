// ReSharper disable UnusedVariable
// ReSharper disable WithExpressionModifiesAllMembers
namespace Immutype.Benchmark;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

[Target]
public record PersonRecordWithFourProperties(string? Name, int Age, string? LastName, bool HasPassport);

[Target]
public readonly record struct PersonRecordStructWithFourProperties(string? Name, int Age, string? LastName, bool HasPassport);

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class RecordWithFourProperties
{
    private readonly PersonRecordWithFourProperties _john = new("John", 15, "Smith", false);
    private readonly PersonRecordStructWithFourProperties _johnStruct = new("John", 15, "Smith", false);

    [Benchmark]
    public void WithForRecord()
    {
        var david = _john with
        {
            Name = "David",
            Age = 17,
            LastName = "Black",
            HasPassport = true
        };
    }

    [Benchmark]
    public void WithForRecordStruct()
    {
        var david = _johnStruct with
        {
            Name = "David",
            Age = 17,
            LastName = "Black",
            HasPassport = true
        };
    }

    [Benchmark]
    public void ImmutypeForRecord()
    {
        var david = _john.WithName("David").WithAge(17).WithLastName("Black").WithHasPassport(true);
    }

    [Benchmark]
    public void ImmutypeForRecordStruct()
    {
        var david = _johnStruct.WithName("David").WithAge(17).WithLastName("Black").WithHasPassport(true);
    }
}