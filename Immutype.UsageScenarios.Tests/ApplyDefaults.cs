// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
namespace Immutype.UsageScenarios.Tests.ApplyDefaults
{
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=01
    // $description=Apply defaults
    // {
    [Immutype.Target]
    internal record Person(string Name = "John", int Age = 17);
    
    public class ApplyDefaults
    {
    // }
        [Fact] 
    // {
        public void Run()
        {
            var john = new Person("David", 15)
                .WithDefaultAge()
                .WithDefaultName();
            
            john.Name.ShouldBe("John");
            john.Age.ShouldBe(17);
        }
    }
    // }
}
