// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
namespace Immutype.UsageScenarios.Tests.Array
{
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=02
    // $description=Array
    // {
    [Immutype.Target]
    internal record Person(string Name, int Age = 0, params Person[] Friends);

    public class Array
    { 
    // }
        [Fact]
    // {
        public void Run()
        {
            var john = new Person("John")
                .WithAge(15)
                .WithFriends(new Person("David").WithAge(16))
                .AddFriends(
                    new Person("James"),
                    new Person("Daniel").WithAge(17));
            
            john.Friends.Length.ShouldBe(3);
        }
    }
    // }
}