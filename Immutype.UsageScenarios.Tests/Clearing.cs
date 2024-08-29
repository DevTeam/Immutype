// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
// ReSharper disable CheckNamespace
// ReSharper disable ArrangeNamespaceBody
// ReSharper disable NotAccessedPositionalProperty.Global
namespace Immutype.UsageScenarios.Tests.Clearing
{
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=02
    // $description=Clearing
    // {
    [Immutype.Target]
    internal readonly record struct Person(
        string Name,
        int Age = 0,
        params Person[] Friends);

    public class Clearing
    {
    // }
        [Fact]
    // {
        public void Run()
        {
            var john = new Person("John",15, new Person("David").WithAge(16))
                .AddFriends(new Person("James"));

            john = john.ClearFriends();
            
            john.Friends.Length.ShouldBe(0);
        }
    }
    // }
}