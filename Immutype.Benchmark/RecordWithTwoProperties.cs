namespace Immutype.Benchmark
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Order;

    [Immutype.Target]
    public record PersonRecordWithTwoProperties(string? Name, int Age);
    
    [Immutype.Target]
    public record struct PersonRecordStructWithTwoProperties(string? Name, int Age);
    
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class RecordWithTwoProperties
    {
        private readonly PersonRecordWithTwoProperties _john = new("John", 15);
        private readonly PersonRecordStructWithTwoProperties _johnStruct = new("John", 15);

        [Benchmark]
        public void WithForRecord()
        {
            var david = _john with { Name = "David", Age = 17 };
        }

        [Benchmark]
        public void WithForRecordStruct()
        {
            var david = _johnStruct with { Name = "David", Age = 17 };
        }

        [Benchmark]
        public void ImmutypeForRecord()
        {
            var david = _john.WithName("David").WithAge(17);
        }
        
        [Benchmark]
        public void ImmutypeForRecordStruct()
        {
            var david = _johnStruct.WithName("David").WithAge(17);
        }
    }
}