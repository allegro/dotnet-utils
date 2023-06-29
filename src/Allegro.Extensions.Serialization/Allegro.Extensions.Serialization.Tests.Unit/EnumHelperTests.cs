using System.Runtime.Serialization;
using Allegro.Extensions.Serialization.Extensions;
using FluentAssertions;
using Xunit;

namespace Allegro.Extensions.Serialization.Tests.Unit;

public class EnumHelperTests
{
    [Theory]
    [InlineData("VALIDATE_FAILED", TestStatus.ValidateFailed)]
    [InlineData("SUCCESS", TestStatus.Success)]
    [InlineData("ValidateFailed", TestStatus.ValidateFailed)]
    [InlineData("ValidationFailedWithoutMember", TestStatus.ValidationFailedWithoutMember)]
    public void Parse_Success(string enumValue, TestStatus expectedStatus)
    {
        var status = enumValue.ToEnum<TestStatus>();
        status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("VALIDATE_FAILED")]
    public void ParseWithDuplicatedEnumMembers_Failed(string enumValue)
    {
        var action = () => enumValue.ToEnum<TestStatusWithDuplicates>();
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("ValidateFailed", TestStatusWithSameValues.ValidateFailed)]
    [InlineData("Success", TestStatusWithSameValues.Success)]
    public void ParseWithSameValuesAsEnumMembers_Passed(string enumValue, TestStatusWithSameValues expectedStatus)
    {
        var status = enumValue.ToEnum<TestStatusWithSameValues>();
        status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(TestStatus.ValidateFailed)]
    [InlineData(TestStatus.Success)]
    public void ParseFromToString_Success(TestStatus expectedStatus)
    {
        var status = expectedStatus.ToString().ToEnum<TestStatus>();
        status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("VALIDATE_FAILED1")]
    [InlineData("SUCCESS1")]
    public void Parse_NotFound(string enumValue)
    {
        var action = () => enumValue.ToEnum<TestStatus>();
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("VALIDATE_FAILED", TestStatus.ValidateFailed)]
    [InlineData("SUCCESS", TestStatus.Success)]
    public void GetEnumMember_SuccessEnumValue(string testEnumValue, TestStatus testEnum)
    {
        var status = testEnum.EnumMemberValue();
        status.Should().Be(testEnumValue);
    }

    [Theory]
    [InlineData("Success", TestStatusWithSameValues.Success)]
    public void GetEnumMemberWithSameValues_SuccessEnumValue(string testEnumValue, TestStatusWithSameValues testEnum)
    {
        var status = testEnum.EnumMemberValue();
        status.Should().Be(testEnumValue);
    }

    [Theory]
    [InlineData(TestStatus.None)]
    public void GetEnumMemberWithoutExisting_Failed(TestStatus testEnum)
    {
        var action = () => testEnum.EnumMemberValue();
        action.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(TestStatusWithDuplicates.ValidateFailed1)]
    [InlineData(TestStatusWithDuplicates.ValidateFailed2)]
    public void GetEnumMemberWithDuplicates_Failed(TestStatusWithDuplicates testEnum)
    {
        var action = () => testEnum.EnumMemberValue();
        action.Should().Throw<ArgumentException>();
    }

    public enum TestStatus
    {
        None,
        ValidationFailedWithoutMember,
        [EnumMember(Value = "VALIDATE_FAILED")]
        ValidateFailed,
        [EnumMember(Value = "SUCCESS")]
        Success,
    }

    public enum TestStatusWithDuplicates
    {
        None,
        [EnumMember(Value = "VALIDATE_FAILED")]
        ValidateFailed1,
        [EnumMember(Value = "VALIDATE_FAILED")]
        ValidateFailed2,
    }

    public enum TestStatusWithSameValues
    {
        None,
        [EnumMember(Value = "ValidateFailed")]
        ValidateFailed,
        [EnumMember(Value = "Success")]
        Success,
    }
}