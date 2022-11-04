// Copyright (c) PlaceholderCompany. All rights reserved.
#pragma warning disable 1591

using System;
using Allegro.Extensions.Identifiers.Abstractions;

namespace Allegro.Extensions.Identifiers.Demo.Identifiers;

[StronglyTypedId(typeof(Guid))]
public partial class OrderId : IStronglyTypedId<Guid>
{
}