
## Usage Scenarios

- Basics
  - [Sample scenario](#sample-scenario)
  - [Array](#array)
  - [Applying defaults](#applying-defaults)
  - [Immutable collection](#immutable-collection)
  - [Removing](#removing)
  - [Generic types](#generic-types)
  - [Nullable collection](#nullable-collection)
  - [Set](#set)
  - [Record with constructor](#record-with-constructor)
  - [Explicit constructor choice](#explicit-constructor-choice)

### Sample scenario



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    bool HasPassport = true,
    int Age = 0,
    ImmutableArray<Person> Friends = default);

public class SampleScenario
{
    public void Run()
    {
        var john = new Person("John", false, 15)
            .AddFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
            
        john.Friends.Length.ShouldBe(2);

        john = john.WithAge(16).WithDefaultHasPassport();
        john.Age.ShouldBe(16);
        john.HasPassport.ShouldBeTrue();

        john = john.AddFriends(
            new Person("Daniel").WithAge(17),
            new Person("Sophia").WithAge(18));
        
        john.Friends.Length.ShouldBe(4);
            
        john = john.RemoveFriends(new Person("David").WithAge(16));

        john.Friends.Length.ShouldBe(3);
    }
}
```



### Array



``` CSharp
[Immutype.Target]
internal record Person(string Name, int Age = 0, params Person[] Friends);

public class Array
{ 
    public void Run()
    {
        var john = new Person("John")
            .WithAge(15)
            .AddFriends(new Person("David").WithAge(16))
            .AddFriends(
                new Person("James"),
                new Person("Daniel").WithAge(17));
        
        john.Friends.Length.ShouldBe(3);
    }
}
```



### Applying defaults



``` CSharp
[Immutype.Target]
internal record Person(string Name = "John", int Age = 17);

public class ApplyingDefaults
{
    public void Run()
    {
        var john = new Person("David", 15)
            .WithDefaultAge()
            .WithDefaultName();
        
        john.Name.ShouldBe("John");
        john.Age.ShouldBe(17);
    }
}
```



### Immutable collection



``` CSharp
[Immutype.Target]
internal readonly struct Person
{
    public readonly string Name;
    public readonly int Age;
    public readonly IImmutableList<Person> Friends;

    public Person(
        string name,
        int age = 0,
        IImmutableList<Person>? friends = default)
    {
        Name = name;
        Age = age;
        Friends = friends ?? ImmutableList<Person>.Empty;
    }
};

public class ImmutableCollection
{
    public void Run()
    {
        var john = new Person("John",15)
            .WithFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17))
            .AddFriends(
                new Person("David").WithAge(22));
        
        john.Friends.Count.ShouldBe(3);
    }
}
```



### Removing



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    int Age = 0,
    params Person[] Friends);

public class Removing
{
    public void Run()
    {
        var john = new Person("John",15, new Person("David").WithAge(16))
            .AddFriends(new Person("James"));

        john = john.RemoveFriends(new Person("James"));
        
        john.Friends.Length.ShouldBe(1);
    }
}
```



### Generic types

It is possible to use generic types including any generic constraints.

``` CSharp
[Immutype.Target]
internal record Person<TAge>(string Name, TAge Age = default, IEnumerable<Person<TAge>>? Friends = default) 
    where TAge : struct;

public class GenericTypes
{ 
    public void Run()
    {
        var john = new Person<int>("John")
            .WithAge(15)
            .WithFriends(new Person<int>("David").WithAge(16))
            .AddFriends(
                new Person<int>("James"),
                new Person<int>("Daniel").WithAge(17));
        
        john.Friends?.Count().ShouldBe(3);
    }
}
```



### Nullable collection



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    int? Age = default,
    ICollection<Person>? Friends = default);

public class NullableCollection
{
    public void Run()
    {
        var john = new Person("John",15)
            .AddFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
        
        john.Friends?.Count.ShouldBe(2);
    }
}
```



### Set



``` CSharp
[Immutype.Target]
internal record Person(
    string Name,
    int Age = 0,
    ISet<Person>? Friends = default);

public class Set
{
    public void Run()
    {
        var john = new Person("John",15)
            .AddFriends(
                new Person("David").WithAge(16),
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
        
        john.Friends?.Count.ShouldBe(2);
    }
}
```



### Record with constructor



``` CSharp
[Immutype.Target]
internal record Person
{
    public Person(
        string name,
        int? age = default,
        ICollection<Person>? friends = default)
    {
        Name = name;
        Age = age;
        Friends = friends;
    }

    public string Name { get; }

    public int? Age { get; }

    public ICollection<Person>? Friends { get; }

    public void Deconstruct(
        out string name,
        out int? age,
        out ICollection<Person>? friends)
    {
        name = Name;
        age = Age;
        friends = Friends;
    }
}

public class RecordWithConstructor
{
    public void Run()
    {
        var john = new Person("John",15)
            .WithFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17)
                    .WithFriends(new Person("Tyler").WithAge(16)));
        
        john.Friends?.Count.ShouldBe(2);
    }
}
```



### Explicit constructor choice



``` CSharp
[Immutype.Target]
internal readonly struct Person
{
    public readonly string Name;
    public readonly int Age;
    public readonly IImmutableList<Person> Friends;

    // You can explicitly select a constructor by marking it with the [Immutype.Target] attribute
    [Immutype.Target]
    public Person(
        string name,
        int age = 0,
        IImmutableList<Person>? friends = default)
    {
        Name = name;
        Age = age;
        Friends = friends ?? ImmutableList<Person>.Empty;
    }
    
    public Person(
        string name,
        int age,
        IImmutableList<Person>? friends,
        int someArg = 99)
    {
        Name = name;
        Age = age;
        Friends = friends ?? ImmutableList<Person>.Empty;
    }
};

public class ExplicitConstructorChoice
{
    public void Run()
    {
        var john = new Person("John",15)
            .WithFriends(
                new Person("David").WithAge(16),
                new Person("James").WithAge(17))
            .AddFriends(
                new Person("David").WithAge(22));
        
        john.Friends.Count.ShouldBe(3);
    }
}
```



