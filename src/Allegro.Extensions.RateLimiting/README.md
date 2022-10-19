# Allegro.Extensions.RateLimiting

RateLimiter allows specifying precise limit of operations performed per configured interval (for example per second). After the limit is reached, the limiter will delay next executions until next interval period. 

## Note on distributed scenarios

Rate limiter has no distributed state. It will work in following scenarios:

- there is only one service instance using rate limiter,
- if there are many instances, each of them will limit the rate individually (for example with 4 replicas and MaxRate=10k the total MaxRate will be 40k ops/s).

## How to use the RateLimiter

When creating a RateLimiter, you specify how many operation units in what interval is allowed. Operation unit 
is an abstract measure of operation cost (such as CosmosDB RU consumption). If all the operations are identical
in terms of cost, or you cannot estimate their cost, you could just assume that each operation is equal to 
1 operation unit. After the limit is reached, the limiter will delay next operations until next interval period 
(see example below).

There are several ways to pass operation cost to the rate limiter:
- If you use `ExecuteAsync` method, the cost is assumed to be `1`. This is useful for simple scenarios such 
as "I want to execute max X operations per Y period".
- If you use `ExecuteWeightedAsync` method, you can pass the cost of the operation using `weight` parameter.
- If you use `ExecuteWithEstimatedWeightAsync` method, you need to pass operation name and a callback function
that will calculate the cost of the operation based on the response of the operation (for example, CosmosDB
returns operation cost in its HTTP response). The RateLimiter keeps a mapping of operation name and their
costs, so the next time you execute the same operation, its cost will already be known. Note: this works
with an assumption that for each operation name, the cost is always the same. RateLimiter remembers the
maximum cost given operation has ever returned.

### Example (operation cost known upfront)
```c#
// creates RateLimiter that allows 12.5 operation units per 2 seconds 
var rateLimiter = new RateLimiter(12.5, TimeSpan.FromSeconds(2));

// executes operation that consumes 10 operation units
rateLimiter.ExecuteWeightedAsync(func, weight: 10);

// executes operation that consumes 2 operation units
rateLimiter.ExecuteWeightedAsync(func, weight: 2);

// executes operation that consumes 1 operation unit
rateLimiter.ExecuteAsync(func); // this will be rate-limited until next 2-second period
```

### Example (simple RPS limiter)
```c#
// creates RateLimiter that allows 5 operations per second 
var rateLimiter = RateLimiter.WithMaxRps(5);

// executes operation that consumes 1 operation unit
rateLimiter.ExecuteAsync(func); // 1
rateLimiter.ExecuteAsync(func); // 2
rateLimiter.ExecuteAsync(func); // 3
rateLimiter.ExecuteAsync(func); // 4
rateLimiter.ExecuteAsync(func); // 5
rateLimiter.ExecuteAsync(func); // 6 - this will be rate-limited until next second
```

### Example (CosmosDB - estimated weight)

One of the RateLimiter use cases is limiting RU usage with Azure CosmosDB. For each operation, CosmosDB
returns its cost in the operation's response. As this is not known upfront, before executing the operation,
we can use a simple estimation. For most use cases it can be based on the assumption, that each operation of
specific type (for example Upsert document in specific collection) has the same or similar cost.

See the CosmosDB implementation for details:  
https://github.com/allegro/cosmosdb-utils/blob/main/src/Allegro.CosmosDb.BatchUtilities/Allegro.CosmosDb.BatchUtilities/BatchRequestHandler.cs#L38
