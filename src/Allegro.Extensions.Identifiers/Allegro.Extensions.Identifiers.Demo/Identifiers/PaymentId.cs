// Copyright (c) PlaceholderCompany. All rights reserved.

using Allegro.Extensions.Identifiers.Abstractions;
using Meziantou.Framework.Annotations;

namespace Allegro.Extensions.Identifiers.Demo.Identifiers;

[StronglyTypedId(typeof(int))]
public partial class PaymentId : IStronglyTypedId<int>
{
}