namespace Allegro.Extensions.Configuration.Exceptions;

public class GlobalContextSectionNameConflictException : Exception
{
    public GlobalContextSectionNameConflictException(
        string contextName,
        string sectionName)
        : base(
            $"[Confeature] Error when loading global context '{contextName}': global contexts " +
            $"with same section name '{sectionName}' already loaded.")
    {
    }
}