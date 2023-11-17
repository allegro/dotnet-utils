using Allegro.Extensions.Configuration.Validation;
using FluentAssertions;
using FluentAssertions.Common;
using Xunit;

namespace Vabank.Confeature.Tests.Unit;

public class DateOverlapValidationTests
{
    [Theory]
#pragma warning disable MA0005
#pragma warning disable CA1825
    [MemberData(nameof(Data))]
#pragma warning restore CA1825
#pragma warning restore MA0005
    public void IsValidTest(bool isValidExpected, IEnumerable<DateRangeTest> dates)
    {
        // arrange
        var validationAttribute = new DateOverlapValidationAttribute("StartDate", "EndDate");

        // act
        var isValid = validationAttribute.IsValid(dates);

        // assert
        isValid.Should().Be(isValidExpected);
    }

    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[]
            {
                true,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 01, 02).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                true,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 03, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 03, 02).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 01, 02).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 04, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 04, 02).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                false,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 03, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 03, 02).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 01, 10).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 03).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 01, 05).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                false,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01, 15, 00, 00).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 02, 02, 15, 00, 00).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 04, 03).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 05, 05).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 02, 02, 10, 00, 00).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 03, 10, 15, 00, 00).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                true,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01, 15, 00, 00).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 02, 02, 15, 00, 00).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 04, 03).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 05, 05).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 02, 02, 15, 00, 01).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 03, 10, 15, 00, 00).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                false,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01, 15, 00, 00).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 02, 02, 15, 00, 00).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 04, 03).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 05, 05).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 02, 01, 15, 00, 01).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                false,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01, 15, 00, 00).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 02, 02, 15, 00, 00).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 04, 03).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 02, 01, 15, 00, 01).ToDateTimeOffset()
                    }
                }
            },
            new object[]
            {
                true,
                new DateRangeTest[]
                {
                    new()
                    {
                        StartDate = new DateTime(2022, 01, 01, 15, 00, 00).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 02, 02, 15, 00, 00).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 04, 03).ToDateTimeOffset()
                    },
                    new()
                    {
                        StartDate = new DateTime(2022, 02, 03).ToDateTimeOffset(),
                        EndDate = new DateTime(2022, 04, 02).ToDateTimeOffset()
                    }
                }
            }
        };

    public class DateRangeTest
    {
        public DateTimeOffset StartDate { get; init; }
        public DateTimeOffset? EndDate { get; init; }
    }
}