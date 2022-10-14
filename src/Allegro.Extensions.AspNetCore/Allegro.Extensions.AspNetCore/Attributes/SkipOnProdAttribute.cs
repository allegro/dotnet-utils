using System;

namespace Allegro.Extensions.AspNetCore.Attributes
{
    /// <summary>
    /// Disables the controller on environments other than test (Environment=dev|uat), unless HostEnvironment is Development.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SkipOnProdAttribute : Attribute
    {
    }
}