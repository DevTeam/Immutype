// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantNameQualifier
namespace Immutype.UsageScenarios.Tests
{
    using System.Collections.Immutable;
    using Shouldly;
    using Xunit;

    // $visible=true
    // $tag=1 Basics
    // $priority=01
    // $description=Simple parameters
    // {
    [Immutype.Target]
    public readonly struct PersonWithMail
    {
        public readonly string FirstName;
        public readonly string SecondName;
        public readonly ImmutableArray<string> Mails;

        public PersonWithMail(string firstName, string secondName, ImmutableArray<string> mails)
        {
            FirstName = firstName;
            SecondName = secondName;
            Mails = mails;
        }
        
        // Additional concise constructor
        public PersonWithMail(string firstName):
            this(firstName, string.Empty, ImmutableArray.Create<string>())
        {
        }
    }

    public class Structs
    {
        [Fact]
        public void Run()
        {
            var person = new PersonWithMail("John").WithSecondName("Smith").AddMails("abc@mail.com", "xyz@mail.com");
            person.SecondName.ShouldBe("Smith");
            person.Mails.ShouldBe(new []{"abc@mail.com", "xyz@mail.com"});
        }
    }
    // }
}
