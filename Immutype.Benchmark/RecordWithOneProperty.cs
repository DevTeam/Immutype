namespace Immutype.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Order;

    [Immutype.Target]
    public record PersonRecordWithOneProperty(string? Name);
    
    [Immutype.Target]
    public record struct PersonRecordStructWithOneProperty(string? Name);
    
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class RecordWithOneProperty
    {
        private readonly PersonRecordWithOneProperty _john = new("John");
        private readonly PersonRecordStructWithOneProperty _johnStruct = new("John");

        [Benchmark]
        public void WithForRecord()
        {
            var david = _john with { Name = "David"};
        }

        [Benchmark]
        public void WithForRecordStruct()
        {
            var david = _johnStruct with { Name = "David"};
        }

        [Benchmark]
        public void ImmutypeForRecord()
        {
            var david = _john.WithName("David");
        }
        
        [Benchmark]
        public void ImmutypeForRecordStruct()
        {
            var david = _johnStruct.WithName("David");
        }
    }
}