using Xunit;

namespace Allegro.Extensions.Configuration.Tests.Integration.Helpers;

/// <summary>
/// This class has no code, and is never instantiated. Its purpose is simply
/// to be the place to apply [CollectionDefinition] and all the
/// ICollectionFixture interfaces.
/// </summary>
[CollectionDefinition(Name)]
#pragma warning disable CA1711
public class CustomWebApplicationFactoryCollection : ICollectionFixture<CustomWebApplicationFactory>
#pragma warning restore CA1711
{
    public const string Name = "Options Registration Validator collection";
}