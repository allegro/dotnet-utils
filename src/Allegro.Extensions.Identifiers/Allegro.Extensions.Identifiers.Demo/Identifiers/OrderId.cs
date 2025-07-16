using Allegro.Extensions.Identifiers.Abstractions;
using Meziantou.Framework.Annotations;

namespace Allegro.Extensions.Identifiers.Demo.Identifiers;

[StronglyTypedId(typeof(Guid))]
public partial class OrderId : IStronglyTypedId<Guid>
{
}