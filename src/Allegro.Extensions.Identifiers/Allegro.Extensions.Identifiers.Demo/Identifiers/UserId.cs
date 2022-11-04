// Copyright (c) PlaceholderCompany. All rights reserved.

using System;
using Allegro.Extensions.Identifiers.Abstractions;

namespace Allegro.Extensions.Identifiers.Demo.Identifiers;

[StronglyTypedId(typeof(string))]
public partial class UserId : IStronglyTypedId<string>
{
    public static UserId Generate() => FromString(Guid.NewGuid().ToString());
}