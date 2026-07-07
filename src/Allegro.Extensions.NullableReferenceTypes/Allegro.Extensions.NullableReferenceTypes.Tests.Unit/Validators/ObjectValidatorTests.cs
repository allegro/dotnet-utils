using Allegro.Extensions.NullableReferenceTypes.Validators;
using AutoBogus;
using FluentAssertions;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Allegro.Extensions.NullableReferenceTypes.Tests.Unit.Validators;

public class ObjectValidatorTests
{
    [Fact]
    public void ShouldPassNonNullableFilled()
    {
        var nonNullable = AutoFaker.Generate<NonNullableObject>();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableListIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableList, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage("Object reference (NonNullableObject.NonNullableList) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableIListIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableIList, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage("Object reference (NonNullableObject.NonNullableIList) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableICollectionIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableICollection, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage("Object reference (NonNullableObject.NonNullableICollection) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableIEnumerableIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableIEnumerable, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage("Object reference (NonNullableObject.NonNullableIEnumerable) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableArrayIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableArray, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage("Object reference (NonNullableObject.NonNullableArray) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableDictionaryIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableDictionary, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage("Object reference (NonNullableObject.NonNullableDictionary) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldPassWhenNonNullableDictionaryOfIEnumerableContainsNullValue() // validator does not support generics NRT
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableDictionaryOfIEnumerable, () => new Dictionary<NonNullableKey, IEnumerable<NonNullableObjectChild>>()
            {
                {
                    AutoFaker.Generate<NonNullableKey>(),
                    null!
                },
            })
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ShouldThrowExceptionWhenIEnumerableElementNonNullableFieldIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableIEnumerable, () => new List<NonNullableObjectChild>
            {
                new()
                {
                    Number = 0,
                    String = null!,
                },
            })
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage(
            "Object reference (NonNullableObject.NonNullableIEnumerable[0].String) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenDictionaryValueNonNullableFieldIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableDictionary, () => new Dictionary<NonNullableKey, NonNullableObjectChild>()
            {
                {
                    new NonNullableKey
                    {
                        Number = 0,
                        String = "key",
                    },
                    new NonNullableObjectChild
                    {
                        Number = 0,
                        String = null!,
                    }
                }
            })
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage(
            "Object reference (NonNullableObject.NonNullableDictionary[NonNullableKey { Number = 0, String = key }].String) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenDictionaryKeyNonNullableFieldIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullableDictionary, () => new Dictionary<NonNullableKey, NonNullableObjectChild>()
            {
                {
                    new NonNullableKey
                    {
                        Number = 0,
                        String = null!,
                    },
                    AutoFaker.Generate<NonNullableObjectChild>()
                },
            })
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage(
            "Object reference (NonNullableObject.NonNullableDictionary[NonNullableKey { Number = 0, String =  }].String) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldThrowExceptionWhenNonNullableObjectFieldIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.NonNullable, () => null!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage(
            "Object reference (NonNullableObject.NonNullable) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldPassWhenNonNullableStringIsNull()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.String, () => default!)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().Throw<NullReferenceException>().WithMessage(
            "Object reference (NonNullableObject.String) not set to an instance of an object.");
    }

    [Fact]
    public void ShouldPassWhenNonNullableStringIsEmpty()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.String, () => string.Empty)
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ShouldPassWhenNonNullableStringIsWhitespace()
    {
        var nonNullable = new AutoFaker<NonNullableObject>()
            .RuleFor(x => x.String, () => "   ")
            .Generate();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ShouldPassNullableObjectFilled()
    {
        var nonNullable = AutoFaker.Generate<NullableObject>();

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ShouldPassNullableObjectWithAllNulls()
    {
        var nonNullable = new NullableObject
        {
            Number = null,
            String = null,
            Date = null,
            Nullable = null,
            NullableIEnumerable = null,
            NullableICollection = null,
            NullableIList = null,
            NullableList = null,
            NullableArray = null,
            NullableIDictionary = null,
            NullableDictionary = null,
        };

        Action action = () => nonNullable.EnsureIsValidObject();
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ShouldBreakOnCircularDependencyOnStaticConstructorMethod()
    {
        var circularDependency = StaticConstructorMethodCircularDependencyObject.Empty;
        Action action = () => circularDependency.EnsureIsValidObject();
        action.Should().NotThrow();
    }

    [Fact]
    public void ShouldSkipPrivateMethods()
    {
        var privateProperties = new PrivatePropertiesObject { PublicText = "Test" };
        Action action = () => privateProperties.EnsureIsValidObject();
        action.Should().NotThrow();
    }

    public record StaticConstructorMethodCircularDependencyObject(decimal Value)
    {
        public static StaticConstructorMethodCircularDependencyObject Empty => new(0);
    }

    private sealed class NonNullableObject
    {
        public int Number { get; init; }

        public required string String { get; init; }

        public required string[] StringArray { get; init; }

        public DateTimeOffset Date { get; init; }

        public required NonNullableObjectChild NonNullable { get; init; }

        public required IEnumerable<NonNullableObjectChild> NonNullableIEnumerable { get; init; }

        public required ICollection<NonNullableObjectChild> NonNullableICollection { get; init; }

        public required IList<NonNullableObjectChild> NonNullableIList { get; init; }

        public required List<NonNullableObjectChild> NonNullableList { get; init; }

        public required NonNullableObjectChild[] NonNullableArray { get; init; }

        public required IDictionary<NonNullableKey, NonNullableObjectChild> NonNullableIDictionary { get; init; }

        public required Dictionary<NonNullableKey, NonNullableObjectChild> NonNullableDictionary { get; init; }

        public required Dictionary<NonNullableKey, IEnumerable<NonNullableObjectChild>> NonNullableDictionaryOfIEnumerable { get; init; }
    }

    private sealed record NonNullableKey
    {
        public int Number { get; init; }

        public required string String { get; init; }
    }

    private sealed class PrivatePropertiesObject
    {
        public required string PublicText { get; init; }

#pragma warning disable IDE0051
        private int Number { get; init; }

        private int? NumberNullable { get; init; }

        private string String { get; init; } = null!;

        private string? StringNullable { get; init; }
#pragma warning restore IDE0051
    }

    private sealed class NonNullableObjectChild
    {
        public int Number { get; init; }

        public required string String { get; init; }
    }

    private sealed class NullableObject
    {
        public int? Number { get; init; }

        public string? String { get; init; }

        public DateTimeOffset? Date { get; init; }

        public NullableObjectChild? Nullable { get; init; }

        public IEnumerable<NullableObjectChild>? NullableIEnumerable { get; init; }

        public ICollection<NullableObjectChild>? NullableICollection { get; init; }

        public IList<NullableObjectChild>? NullableIList { get; init; }

        public List<NullableObjectChild>? NullableList { get; init; }

        public NullableObjectChild[]? NullableArray { get; init; }

        public IDictionary<NullableKey, NullableObjectChild>? NullableIDictionary { get; init; }

        public Dictionary<NullableKey, NullableObjectChild>? NullableDictionary { get; init; }

        public Dictionary<NonNullableKey, IEnumerable<NonNullableObjectChild>?>? NullableDictionaryOfIEnumerable { get; init; }
    }

    private sealed class NullableKey
    {
        public int? Number { get; init; }

        public string? String { get; init; }
    }

    private sealed class NullableObjectChild
    {
        public int? Number { get; init; }

        public string? String { get; init; }
    }
}