using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ArgumentsStyleNamedExpression
namespace Allegro.Extensions.Guid.Tests.Unit;

public class FGuidTests
{
    private readonly ITestOutputHelper _output;

    public FGuidTests(ITestOutputHelper output)
    {
        _output = output;
        FGuid.ResetOptions();
    }

    [Fact]
    public void Examples()
    {
        _output.WriteLine($"Generate(): {FGuid.Generate()}");
    }

    [Fact]
    public void ExamplesStruct()
    {
        _output.WriteLine($"Generate(): {FGuid.NewGuid()}");
    }

    [Theory]
    [InlineData("1234", true)]
    [InlineData(" 1234 ", true)]
    [InlineData("1-234", false)]
    [InlineData("1a234", false)]
    public void Parse(string toParse, bool expected)
    {
        var parsed = FGuid.TryParse(toParse, out _);
        parsed.Should().Be(expected);
    }

    [Fact]
    public void CompareStructs()
    {
        var fguid = FGuid.NewGuid();
        var fguidToString = fguid.ToString();
        var fguidFromString = FGuid.Parse(fguidToString);
        var fguidRandomNumber = FGuid.Parse("1234");
        fguid.Should().Be(fguidFromString);
        fguid.Should().NotBe(fguidToString); // different type
        fguid.Should().NotBe(fguidRandomNumber); // different value
        // TODO: more comparision
    }

    [Fact]
    public void ConfigurationNotChanged()
    {
        var configuration = FGuid.GetFormattedConfiguration();
        _output.WriteLine($"GetFormatted: {configuration}");
        configuration.Should().NotContain("192.168.1.1");
    }

    [Fact]
    public void NoReasonableCollision()
    {
        var l = new List<string>();

        for (var i = 0; i <= 1_200_000; i++)
        {
            var gen = FGuid.Generate();
            l.Add(gen);
        }

        l.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(1_200_000)]
#pragma warning disable xUnit1028
    public IList<long> NoReasonableCollisionLong(int count)
#pragma warning restore xUnit1028
    {
        var l = new List<long>();

        for (var i = 0; i <= count; i++)
        {
            var gen = FGuid.GenerateLong();
            l.Add(gen);
        }

        l.Should().OnlyHaveUniqueItems();

        return l;
    }

    [Fact]
    public void NoReasonableCollisionLong_MultiThread()
    {
        var range = Enumerable.Range(1, 4);
        var results = new List<IEnumerable<long>>();

        Parallel.ForEach(
            range,
            _ =>
            {
                _output.WriteLine(
                    $"Thread: {Environment.CurrentManagedThreadId}, Configuration: {FGuid.GetFormattedConfiguration}");
                var l = NoReasonableCollisionLong(300_000);
                _output.WriteLine($"count: {l.Count}");

                results.Add(l);
            });

        var sum = results.SelectMany(list => list).ToList();
        _output.WriteLine($"sum count: {sum.Count}");

        sum.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void FGuidsAreShorterThanGuids()
    {
        var guid = System.Guid.NewGuid().ToString().Replace("-", string.Empty);
        var guidLength = guid.Length;
        var fguid = FGuid.Generate();
        var fguidLength = fguid.Length;

        _output.WriteLine($"Guid: Len: {guidLength} : {guid}");
        _output.WriteLine($"Guid: Len: {fguidLength} : {fguid}");

        guidLength.Should().Be(32);
        fguidLength.Should().Be(19);
    }

    [Fact]
    public void TestMaxLength()
    {
        FGuid.GetMaxLength().Should().Be(19);
    }

    [Theory]
    [InlineData("123.123.0.1", 1)]
    [InlineData("123.123.0.10", 10)]
    [InlineData("123.123.1.12", 268)]
    [InlineData("123.123.89.47", 303)]
    [InlineData("123.123.256.47", 47)]
    public void GivenValue_ShouldReturnExpectedResult(string ip, int value)
    {
        var seq = FGuid.GetSequenceNumber(ip, 21);
        seq.Should().Be(value);
    }

    [Fact]
    public void MovingUp_ShouldMoveSequence()
    {
        var prefix = "123.123";
        var max = 256;
        var netmask = 21;

        for (var i = 0; i <= 4; i++)
        {
            for (var j = 0; j < max; j++)
            {
                var seq = FGuid.GetSequenceNumber($"{prefix}.{i}.{j}", netmask);
                seq.Should().Be((i * max) + j);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    [InlineData("123.123")]
    [InlineData("123")]
    [InlineData("123.123.123")]
    [InlineData("123.123.123.123.123")]
    [InlineData("abc.123.123.123")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void GivenValue_ShouldThrowException(string ip)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FGuid.GetSequenceNumber(ip, 21));
    }

    [Theory]
    [InlineData(0b_00, 0b_11111111111, 0b_00_11111111111)]
    [InlineData(0b_01, 0b_11111111111, 0b_01_11111111111)]
    [InlineData(0b_10, 0b_11111011011, 0b_10_11111011011)]
    [InlineData(0b_11, 0b_10101010101, 0b_11_10101010101)]
    [InlineData(0b_1000, 0b_11111011011, 0b_10_0011111011011)]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 2048)]
    [InlineData(2, 0, 4096)]
    public void ShouldAddClusterIdToSequence(int clusterId, int sequence, int result)
    {
        FGuid.AddClusterNumber(clusterId, sequence, 11).Should().Be(result);
    }
}

public class FGuidConfigurationTests
{
    private readonly ITestOutputHelper _output;

    public FGuidConfigurationTests(ITestOutputHelper output)
    {
        _output = output;
        FGuid.ResetOptions();
    }

    [Fact]
    public void ChangeConfiguration()
    {
        FGuid.SetOptions(
            new(
                new GeneratorIdStructureOptions(
                    TimestampBits: GeneratorDefaultOptions.TimestampBits,
                    GeneratorIpBits: GeneratorDefaultOptions.GeneratorIpBits,
                    GeneratorClusterBits: GeneratorDefaultOptions.GeneratorClusterBits,
                    SequenceBits: GeneratorDefaultOptions.SequenceBits),
                new GeneratorMachineOptions(
                    Ip: "192.168.1.1",
                    Netmask: 20,
                    ClusterNumber: 1)));

        var configuration = FGuid.GetFormattedConfiguration();
        _output.WriteLine($"GetFormatted: {configuration}");
        configuration.Should().Contain("192.168.1.1");
    }
}