#nullable enable
namespace Vabank.Storage.Postgres.Sdk.Tests.Integration.Models;

public class TestModel
{
    public TestModel(int id, string? username)
    {
        Id = id;
        Username = username;
    }

    public int Id { get; set; }
    public string? Username { get; set; }
}