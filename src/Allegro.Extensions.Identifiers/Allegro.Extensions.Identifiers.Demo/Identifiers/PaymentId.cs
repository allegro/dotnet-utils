// Copyright (c) PlaceholderCompany. All rights reserved.

using Allegro.Extensions.Identifiers.Abstractions;

namespace Allegro.Extensions.Identifiers.Demo.Identifiers;

[StronglyTypedId(typeof(int))]
public partial class PaymentId : IStronglyTypedId<int>
{
}