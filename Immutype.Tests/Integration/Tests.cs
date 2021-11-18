using Shouldly;
using Xunit;
// ReSharper disable StringLiteralTypo

namespace Immutype.Tests.Integration
{
    public class Tests
    {
        [Fact]
        public void ShouldCreateRecordWithNoValues()
        {
            // Given
            const string statements = "System.Console.WriteLine(new Rec());";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.Target]
                public record Rec();
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "Rec { }" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordWithSingleValue()
        {
            // Given
            const string statements = "System.Console.WriteLine(new Rec(33).WithVal(99));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.TargetAttribute()]
                public record Rec(int val);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "Rec { val = 99 }" }, generatedCode);
        }

        [Fact]
        public void ShouldCreateRecordWith2Values()
        {
            // Given
            const string statements = "System.Console.WriteLine(new Rec(33, \"Abc\").WithVal(99).WithStr(\"Xyz\"));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using Immutype;
                
                [Target]
                public record Rec(int val, string str);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "Rec { val = 99, str = Xyz }" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordWithSingleIEnumerable()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).WithVals(99, 66).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IEnumerable<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "99,66" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleIEnumerable()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IEnumerable<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddComplexIEnumerable()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33, 77}, 66).WithVal(55).AddVals(99).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IEnumerable<int> vals, int val);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "33,77,99" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleIReadOnlyCollection()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IReadOnlyCollection<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleIReadOnlyList()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IReadOnlyList<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleICollection()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IList<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleIList()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[] {33}).AddVals(99,44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(IList<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleList()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new List<int>{33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(List<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleSystemCollectionsGenericList()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new List<int>{33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(System.Collections.Generic.List<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateRecordAddSingleArray()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(new[]{33}).AddVals(99, 44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Generic;
                using Immutype;

                [Target]
                public record Rec(int[] vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "33,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateStructWithSingleValue()
        {
            // Given
            const string statements = "System.Console.WriteLine(new Rec(33).WithVal(99));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.TargetAttribute()]
                public readonly struct Rec
                {
                    public readonly int Val;
                    public Rec(int val) { Val = val; }
                }
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "Sample.Rec" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateClassWithSingleValue()
        {
            // Given
            const string statements = "System.Console.WriteLine(new Rec(33).WithVal(99));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.TargetAttribute()]
                public class Rec
                {
                    public readonly int Val;
                    public Rec(int val) { Val = val; }
                }
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "Sample.Rec" }, generatedCode);
        }
        
        [Fact]
        public void ShouldCreateClassWithValues()
        {
            // Given
            const string statements = "System.Console.WriteLine(new Rec(33, 99).WithVal1(99));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                
                [Immutype.TargetAttribute()]
                public class Rec
                {
                    public readonly int Val1;
                    public readonly int Val2;
                    public Rec(int val1, int val2)
                    { 
                        Val1 = val1; 
                        Val2 = val2;
                    }
                }
            }".Run(out var generatedCode, new RunOptions { Statements = statements });

            // Then
            output.ShouldBe(new [] { "Sample.Rec" }, generatedCode);
        }
        
        [Fact]
        public void ShouldSupportImmutableList()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(ImmutableList.Create(33)).WithVals(22, 55).AddVals(99,44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Immutable;
                using Immutype;

                [Target]
                public record Rec(ImmutableList<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "22,55,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldSupportIImmutableList()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(ImmutableList.Create(33)).WithVals(22, 55).AddVals(99,44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Immutable;
                using Immutype;

                [Target]
                public record Rec(IImmutableList<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "22,55,99,44" }, generatedCode);
        }
        
        [Fact]
        public void ShouldSupportImmutableArray()
        {
            // Given
            const string statements = "System.Console.WriteLine(string.Join(',', new Rec(ImmutableArray.Create(33)).WithVals(22, 55).AddVals(99,44).vals));";

            // When
            var output = @"
            namespace Sample
            {
                using System;
                using System.Collections.Immutable;
                using Immutype;

                [Target]
                public record Rec(ImmutableArray<int> vals);
            }".Run(out var generatedCode, new RunOptions { Statements = statements });
            
            // Then
            output.ShouldBe(new [] { "22,55,99,44" }, generatedCode);
        }
    }
}