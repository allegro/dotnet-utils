// Copyright (c) PlaceholderCompany. All rights reserved.

using Allegro.Extensions.Identifiers.Abstractions;
using Meziantou.Framework.Annotations;

namespace Allegro.Extensions.Identifiers.Demo.Identifiers;

[StronglyTypedId(typeof(string))]
public partial class UserId : IStronglyTypedId<string>
{
    public static UserId Generate() => FromString(Guid.NewGuid().ToString());
}