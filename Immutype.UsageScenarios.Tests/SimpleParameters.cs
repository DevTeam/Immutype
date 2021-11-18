// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
namespace Immutype.UsageScenarios.Tests
{
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=01
    // $description=Simple parameters
    // {
    [Immutype.Target]
    internal record Person(string FirstName, string SecondName = "");
    
    public class SimpleParameters
    {
        [Fact]
        public void Run()
        {
            var person = new Person("John").WithSecondName("Smith");
            person.SecondName.ShouldBe("Smith");
        }
    }
    // }
}
