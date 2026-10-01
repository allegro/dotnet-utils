namespace Allegro.Extensions.Configuration.DataContracts;

/// <summary>
/// Get secrets request
/// </summary>
public class GetSecretsRequest
{
    /// <summary>
    /// List of key vault prefixes to include in response
    /// </summary>
    public List<string> KeyVaultPrefixes { get; init; } = new();
}