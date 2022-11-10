using System;

namespace Allegro.Extensions.Cqrs.Abstractions;

/// <summary>
/// Marker attribute - allows to mark ICommandHandler, IQueryHandler as decorator (pattern) of main handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DecoratorAttribute : Attribute
{
}