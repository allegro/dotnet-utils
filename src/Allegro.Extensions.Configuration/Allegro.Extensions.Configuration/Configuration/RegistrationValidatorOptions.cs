namespace Allegro.Extensions.Configuration.Configuration;

public class RegistrationValidatorOptions
{
    public List<string> AssemblyPrefixesToValidate { get; set; } = new();
    public List<string> NamespacesToIgnore { get; set; } = new();
}