# Allegro.Extensions.Identifiers

In most scenarios in microservice architecture (but not only) in communication between services some kind of identifiers are passed.

Commonly, they are passed in body or in url as string ex.:

`http://serviceName/{userid}/resource/{id}`

or

```json
{
    "userId": "value",
    "paymentId": "value"
}
```

Additionally in code it might be passed like:

```c#
Task ExecuteCommand(string userId, string paymentId) { ... }
```

In this case compiler is not defending as from switching parameters by mistake in wrong order (both are strings) that might cause serious issues.
Primitive obsession code smell teaches as to wrap our primitive data structure into strongly typed objects to prevent from this kind of issues.

This is why we try to use strongly typed identifiers in our code as fast (high) as possible, ex.:

```c#
Task ExecuteCommand(UserId userId, PaymentId paymentId) { ... }
```


## Allegro.Extensions.Identifiers.Abstractions

To standardize our approach between services we introduced marker interface for strongly typed identifier:

```c#
public interface IStronglyTypedId<out T>
{
    public T Value { get; }

    public string ValueAsString { get; }
}
```

To be able to use them in api or store in db without additional data structure we want still to use string in api but use strongly typed in code base:

`http://serviceName/123123123/payemnt/13af7673-593c-4b7f-9d99-7c45faadb1e1`

```c#
[Get("{userId}/payemnt/{paymentId}")]
public Task Action([FromRoute] UserId userId,[FromRoute] PaymentId paymentId) { ... }
```

To achieve that we use code generation framework delivered by package [Meziantou.Framework.StronglyTypedId](https://github.com/meziantou/Meziantou.Framework/tree/main/src/Meziantou.Framework.StronglyTypedId#readme) with usage:

```c#
[StronglyTypedId(typeof(Guid))]
public partial class PaymentId : IStronglyTypedId<Guid>
{
}
```

More examples can be found in [demo](Allegro.Extensions.Identifiers.Demo).


## Allegro.Extensions.Identifiers.AspNetCore

To be able to use strongly typed identifiers in api/contracts and still use swagger for testing purposes some code hints need to be added to learn swagger how to interpret them.

In this package you can find extensions to support strongly type identifiers by swagger. They are based on marker interface `IStronglyTypedId<out T>`.

Usage can be found here [demo](Allegro.Extensions.Identifiers.Demo/Startup.cs).