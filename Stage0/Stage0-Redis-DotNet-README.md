# Redis + .NET — Stage 0: Redis Fundamentals

## Overview

This project is **Stage 0** of the Redis + .NET learning path.

The purpose of Stage 0 is to build a strong foundation before moving into Redis data structures, caching patterns, distributed systems, and production architecture.

The project uses:

- C#
- ASP.NET Core Web API
- .NET 10
- Redis 7
- Docker
- StackExchange.Redis
- System.Text.Json

The Stage 0 project is intentionally simple. It exposes Redis operations directly through an ASP.NET Core API so that the Redis concepts remain visible while learning.

> **Important:** Stage 1 is intentionally implemented as a separate project. Stage 0 should remain unchanged as a reference implementation.

---

# 1. Learning Objectives

By completing Stage 0, you should understand:

- What Redis is
- What Redis is not
- How Redis stores keys and values
- Redis Strings
- Redis key naming
- TTL and key expiration
- Redis logical databases
- `IConnectionMultiplexer`
- `IDatabase`
- StackExchange.Redis
- Redis from .NET
- Serialization and deserialization
- Storing structured objects in Redis
- `KEYS` vs `SCAN`
- `MGET`
- Redis memory inspection
- Redis failure behavior
- Basic cache concepts
- Cache invalidation
- Why Redis should not automatically be treated as a database replacement

---

# 2. Project Structure

The Redis learning directory contains separate projects for each stage.

```text
RedisLearning/
│
├── RedisPlayground.Api/
│   └── Stage 0
│
└── RedisDataStructuresApi/
    └── Stage 1
```

Stage 0 project:

```text
RedisPlayground.Api/
│
├── Controllers/
│   ├── RedisController.cs
│   └── ProductController.cs
│
├── Models/
│   └── Product.cs
│
├── Program.cs
├── appsettings.json
└── RedisPlayground.Api.csproj
```

Redis infrastructure:

```text
RedisLearning/
│
└── docker-compose.yml
```

---

# 3. Architecture

The basic Stage 0 architecture is:

```text
                  HTTP Request
                       │
                       ▼
              ┌─────────────────┐
              │ ASP.NET Core API│
              │ .NET 10          │
              └────────┬────────┘
                       │
                       ▼
             StackExchange.Redis
                       │
                       ▼
              ┌─────────────────┐
              │      Redis      │
              │ Docker :6379    │
              └─────────────────┘
```

For structured objects:

```text
C# Object
    │
    ▼
System.Text.Json
    │
    ▼
JSON String
    │
    ▼
Redis
```

When reading:

```text
Redis
  │
  ▼
JSON String
  │
  ▼
System.Text.Json
  │
  ▼
C# Object
```

---

# 4. Prerequisites

Install:

- .NET 10 SDK
- Docker Desktop
- Docker Compose

Verify .NET:

```bash
dotnet --version
```

Verify Docker:

```bash
docker --version
```

Verify Docker Compose:

```bash
docker compose version
```

---

# 5. Create the Project

Create the learning directory:

```bash
mkdir RedisLearning
cd RedisLearning
```

Create the API:

```bash
dotnet new webapi -n RedisPlayground.Api
```

Enter the project:

```bash
cd RedisPlayground.Api
```

Install StackExchange.Redis:

```bash
dotnet add package StackExchange.Redis
```

Return to the root:

```bash
cd ..
```

---

# 6. Run Redis with Docker

Create:

```text
docker-compose.yml
```

Use:

```yaml
services:

  redis:
    image: redis:7
    container_name: redis-playground
    ports:
      - "6379:6379"
    restart: unless-stopped
```

Start Redis:

```bash
docker compose up -d
```

Verify:

```bash
docker ps
```

You should see:

```text
redis-playground
```

Redis is available at:

```text
localhost:6379
```

---

# 7. Connect to Redis CLI

Run:

```bash
docker exec -it redis-playground redis-cli
```

You should see:

```text
127.0.0.1:6379>
```

Test the connection:

```redis
PING
```

Expected:

```text
PONG
```

Exit:

```redis
exit
```

---

# 8. Basic Redis Commands

## SET

Store a value:

```redis
SET name Ajay
```

Response:

```text
OK
```

## GET

Retrieve it:

```redis
GET name
```

Response:

```text
"Ajay"
```

## EXISTS

Check whether a key exists:

```redis
EXISTS name
```

Response:

```text
(integer) 1
```

A non-existent key:

```redis
EXISTS xyz
```

Response:

```text
(integer) 0
```

## DELETE

Delete:

```redis
DEL name
```

Verify:

```redis
GET name
```

Result:

```text
(nil)
```

---

# 9. Redis Keys

Redis keys are strings.

Example:

```redis
SET user:1001:name Ajay
```

The colon does not represent an actual hierarchy.

Redis simply sees:

```text
"user:1001:name"
```

The colon is a naming convention.

Common examples:

```text
user:1001:name
user:1001:email
product:1001
order:50001
session:abc123
```

A common convention is:

```text
<entity>:<identifier>:<property>
```

or:

```text
<purpose>:<identifier>
```

Key naming becomes increasingly important as the number of cached objects grows.

---

# 10. TTL and Expiration

A Redis key can have an expiration time.

Create a key:

```redis
SET otp:123456 987654
```

Check TTL:

```redis
TTL otp:123456
```

If there is no expiration:

```text
(integer) -1
```

Add a 60-second expiration:

```redis
EXPIRE otp:123456 60
```

Check again:

```redis
TTL otp:123456
```

The result will be approximately:

```text
(integer) 60
```

The number decreases over time.

After expiration:

```redis
GET otp:123456
```

returns:

```text
(nil)
```

---

# 11. SET With Expiration

Instead of:

```redis
SET otp:123456 987654
EXPIRE otp:123456 60
```

you can use:

```redis
SET otp:123456 987654 EX 60
```

This establishes the value and expiration together.

---

# 12. TTL Return Values

Redis TTL has important return values.

### Positive number

```text
60
```

The key exists and expires in approximately 60 seconds.

### `-1`

```text
-1
```

The key exists but has no expiration.

### `-2`

```text
-2
```

The key does not exist.

---

# 13. Remove Expiration

Create a key with expiration:

```redis
SET test hello EX 60
```

Remove its expiration:

```redis
PERSIST test
```

Check:

```redis
TTL test
```

The result should be:

```text
(integer) -1
```

The key still exists, but it no longer expires.

---

# 14. Redis Logical Databases

Redis can provide multiple logical databases.

Check the configured number:

```redis
CONFIG GET databases
```

A typical result is:

```text
1) "databases"
2) "16"
```

Switch to database 1:

```redis
SELECT 1
```

Create a key:

```redis
SET test hello
```

Switch back:

```redis
SELECT 0
```

Then:

```redis
GET test
```

returns:

```text
(nil)
```

because the key exists in database 1, not database 0.

## Production guidance

Do not use:

```text
DB 0 → Development
DB 1 → Test
DB 2 → Production
```

as your environment isolation strategy.

Prefer separate Redis infrastructure for separate environments:

```text
Development
    ↓
Development Redis

Test
    ↓
Test Redis

Production
    ↓
Production Redis
```

Logical databases do not provide the isolation normally expected between environments.

---

# 15. Configure Redis in .NET

Open:

```text
appsettings.json
```

Add:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

---

# 16. Register StackExchange.Redis

`Program.cs`:

```csharp
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString =
    builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException(
        "Redis connection string is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

---

# 17. IConnectionMultiplexer

`IConnectionMultiplexer` represents the Redis connection infrastructure.

It should generally be registered as a singleton and reused.

Conceptually:

```text
ASP.NET Core
     │
     ▼
IConnectionMultiplexer
     │
     ├── Connection management
     ├── Redis servers
     ├── Pub/Sub
     └── Database access
```

Do not create a new Redis connection for every HTTP request.

Avoid patterns such as:

```csharp
var connection =
    ConnectionMultiplexer.Connect("localhost:6379");
```

inside every controller method.

Instead:

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(...);
```

and inject it.

---

# 18. IDatabase

Obtain a Redis database from the multiplexer:

```csharp
private readonly IDatabase _redis;

public RedisController(
    IConnectionMultiplexer connectionMultiplexer)
{
    _redis = connectionMultiplexer.GetDatabase();
}
```

Then execute Redis commands through `IDatabase`.

Examples:

```csharp
await _redis.StringSetAsync("name", "Ajay");

var value =
    await _redis.StringGetAsync("name");
```

---

# 19. Basic Redis Controller

Example:

```csharp
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisPlayground.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly IDatabase _redis;

    public RedisController(
        IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    [HttpPost("set")]
    public async Task<IActionResult> Set(
        string key,
        string value)
    {
        await _redis.StringSetAsync(key, value);

        return Ok(new
        {
            Key = key,
            Value = value
        });
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get(string key)
    {
        var value =
            await _redis.StringGetAsync(key);

        if (value.IsNull)
        {
            return NotFound();
        }

        return Ok(new
        {
            Key = key,
            Value = value.ToString()
        });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(string key)
    {
        var deleted =
            await _redis.KeyDeleteAsync(key);

        return Ok(new
        {
            Key = key,
            Deleted = deleted
        });
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(string key)
    {
        var exists =
            await _redis.KeyExistsAsync(key);

        return Ok(new
        {
            Key = key,
            Exists = exists
        });
    }

    [HttpPost("expiration")]
    public async Task<IActionResult> SetExpiration(
        string key,
        int seconds)
    {
        var exists =
            await _redis.KeyExistsAsync(key);

        if (!exists)
        {
            return NotFound();
        }

        await _redis.KeyExpireAsync(
            key,
            TimeSpan.FromSeconds(seconds));

        return Ok(new
        {
            Key = key,
            ExpirationSeconds = seconds
        });
    }

    [HttpGet("ttl")]
    public async Task<IActionResult> GetTtl(string key)
    {
        var ttl =
            await _redis.KeyTimeToLiveAsync(key);

        return Ok(new
        {
            Key = key,
            Ttl = ttl?.TotalSeconds
        });
    }
}
```

---

# 20. API Operations

The basic API supports:

```text
POST   /api/redis/set
GET    /api/redis/get
DELETE /api/redis/delete
GET    /api/redis/exists
POST   /api/redis/expiration
GET    /api/redis/ttl
```

Example:

```http
POST /api/redis/set?key=user:1001:name&value=Ajay
```

Then:

```http
GET /api/redis/get?key=user:1001:name
```

Result:

```json
{
  "key": "user:1001:name",
  "value": "Ajay"
}
```

---

# 21. Storing Structured Objects

Redis itself doesn't understand C# classes.

Given:

```csharp
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
```

Serialize it:

```csharp
var json =
    JsonSerializer.Serialize(product);
```

Store it:

```csharp
await _redis.StringSetAsync(
    "product:1001",
    json);
```

Read it:

```csharp
var value =
    await _redis.StringGetAsync(
        "product:1001");
```

Deserialize:

```csharp
var product =
    JsonSerializer.Deserialize<Product>(
        value.ToString());
```

The flow is:

```text
C# Product
     ↓
JsonSerializer
     ↓
JSON
     ↓
Redis String
```

---

# 22. Product Controller

Example:

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RedisPlayground.Api.Models;
using StackExchange.Redis;

namespace RedisPlayground.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IDatabase _redis;

    public ProductController(
        IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    [HttpPost]
    public async Task<IActionResult> SetProduct(
        Product product)
    {
        var key =
            $"product:{product.Id}";

        var json =
            JsonSerializer.Serialize(product);

        await _redis.StringSetAsync(
            key,
            json,
            TimeSpan.FromMinutes(10));

        return Ok(new
        {
            Key = key,
            Product = product
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var key =
            $"product:{id}";

        var json =
            await _redis.StringGetAsync(key);

        if (json.IsNullOrEmpty)
        {
            return NotFound();
        }

        var product =
            JsonSerializer.Deserialize<Product>(
                json.ToString());

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(
        int id)
    {
        var key =
            $"product:{id}";

        var deleted =
            await _redis.KeyDeleteAsync(key);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
```

---

# 23. Product Model

```csharp
namespace RedisPlayground.Api.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
```

---

# 24. Test Structured Data

Request:

```http
POST /api/products
```

Body:

```json
{
  "id": 1001,
  "name": "Mechanical Keyboard",
  "price": 4999.99,
  "category": "Electronics",
  "isActive": true
}
```

Redis stores something conceptually similar to:

```text
product:1001
    ↓
{
    "Id": 1001,
    "Name": "Mechanical Keyboard",
    "Price": 4999.99,
    "Category": "Electronics",
    "IsActive": true
}
```

Redis doesn't know this is a C# `Product`.

It stores the serialized representation.

---

# 25. Inspect the Stored Object

Open Redis CLI:

```bash
docker exec -it redis-playground redis-cli
```

Run:

```redis
GET product:1001
```

You should see the JSON representation.

Check the key type:

```redis
TYPE product:1001
```

Result:

```text
string
```

This is important:

> A JSON document stored using `StringSetAsync` is still a Redis String.

Redis does not automatically understand the JSON structure.

---

# 26. KEYS vs SCAN

You can list keys using:

```redis
KEYS *
```

This is acceptable for a small development database.

However, avoid casually using:

```redis
KEYS *
```

against a large production keyspace.

If Redis contains millions of keys, scanning the entire keyspace with `KEYS` can cause operational problems.

Use:

```redis
SCAN 0
```

instead.

You can also use:

```redis
SCAN 0 MATCH product:*
```

`SCAN` iterates incrementally.

Conceptually:

```text
KEYS
 ↓
Potentially expensive keyspace operation

SCAN
 ↓
Incremental iteration
```

---

# 27. SCAN Is Not an Application Query Mechanism

Do not design your application like:

```text
Find all products
     ↓
SCAN product:*
     ↓
Return products
```

For application-level queries, design appropriate keys and data structures.

`SCAN` is more appropriate for:

- administration
- maintenance
- migration
- debugging
- operational tooling

---

# 28. MGET

If you need several String values, Redis supports multi-key retrieval.

Instead of:

```redis
GET product:1001
GET product:1002
GET product:1003
```

you can use:

```redis
MGET product:1001 product:1002 product:1003
```

From .NET:

```csharp
var keys = new RedisKey[]
{
    "product:1001",
    "product:1002",
    "product:1003"
};

var values =
    await _redis.StringGetAsync(keys);
```

This can reduce network round trips compared with sequential individual calls.

---

# 29. Memory Inspection

Inspect an individual key:

```redis
MEMORY USAGE product:1001
```

Inspect Redis memory information:

```redis
INFO memory
```

Useful fields include:

```text
used_memory
used_memory_human
maxmemory
maxmemory_policy
```

Redis is memory-oriented, so payload size matters.

For example:

```text
1 KB × 1,000,000 objects
≈ 1 GB
```

This is a simplified calculation and does not include Redis's internal memory overhead.

---

# 30. Redis Data Is Not Automatically Durable

Our Docker setup currently uses:

```yaml
services:
  redis:
    image: redis:7
    container_name: redis-playground
    ports:
      - "6379:6379"
```

There is no explicit persistence configuration in this learning setup.

This is deliberate.

Later stages will cover:

- RDB
- AOF
- persistence trade-offs
- replication
- Sentinel
- Cluster
- recovery

Do not assume that every Redis deployment has the same durability guarantees.

---

# 31. Intentionally Stop Redis

Start the API and Redis.

Then:

```bash
docker stop redis-playground
```

Call an API endpoint that requires Redis.

You will encounter a Redis connection/timeout-related failure.

This demonstrates:

```text
API
 ↓
Redis
 ↓
Redis unavailable
 ↓
Application failure
```

The correct production behavior depends on why Redis is being used.

For a cache:

```text
Redis unavailable
     ↓
Potential fallback
     ↓
Database
```

For a distributed lock:

```text
Redis unavailable
     ↓
Potentially fail operation
```

There is no universal "just retry Redis" answer.

Resilience is workload-specific.

---

# 32. Restart Redis

```bash
docker start redis-playground
```

Verify:

```bash
docker ps
```

Test:

```bash
docker exec -it redis-playground redis-cli
```

Then:

```redis
PING
```

Expected:

```text
PONG
```

---

# 33. Cache Invalidation

Consider:

```text
SQL Server
Product 1001
Price = 5000
```

Redis:

```text
product:1001
Price = 5000
```

Now SQL changes:

```text
Price = 5500
```

Redis doesn't automatically know.

It can continue serving:

```text
Price = 5000
```

This is the cache invalidation problem.

Possible approaches include:

### TTL

Let the cache expire.

### Explicit invalidation

```text
UPDATE SQL
    ↓
DEL product:1001
```

### Cache update

```text
UPDATE SQL
    ↓
SET product:1001 new value
```

### Event-driven invalidation

```text
Database change
      ↓
Event
      ↓
Cache invalidation
```

These approaches will be studied in later stages.

---

# 34. Important Redis Mental Model

Do not think of Redis simply as:

> "A faster SQL Server."

A better model is:

> Redis is an in-memory data structure server that can be used for caching, ephemeral state, coordination, counters, queues, messaging, and other workloads.

For many applications:

```text
SQL Server
    ↓
System of Record
```

while:

```text
Redis
    ↓
Fast / derived / temporary state
```

is a useful architectural model.

Redis does support persistence, but that does not mean every Redis workload should be designed as a relational database replacement.

---

# 35. Stage 0 Commands Reference

## Strings

```redis
SET key value
GET key
DEL key
EXISTS key
```

## Expiration

```redis
EXPIRE key seconds
TTL key
PERSIST key
SET key value EX seconds
```

## Database

```redis
SELECT 0
SELECT 1
```

## Inspection

```redis
TYPE key
KEYS *
SCAN 0
MEMORY USAGE key
INFO memory
```

## Multiple Values

```redis
MGET key1 key2 key3
```

---

# 36. Stage 0 .NET API Reference

### Connection

```csharp
IConnectionMultiplexer
```

### Database

```csharp
IDatabase
```

### Strings

```csharp
StringSetAsync()
StringGetAsync()
```

### Keys

```csharp
KeyDeleteAsync()
KeyExistsAsync()
```

### Expiration

```csharp
KeyExpireAsync()
KeyTimeToLiveAsync()
```

### Multiple values

```csharp
StringGetAsync(RedisKey[])
```

---

# 37. Stage 0 Exercises

Before moving to Stage 1, complete the following.

## Exercise 1 — Basic Operations

Implement:

```text
POST   /api/redis/set
GET    /api/redis/get
DELETE /api/redis/delete
GET    /api/redis/exists
POST   /api/redis/expiration
GET    /api/redis/ttl
```

---

## Exercise 2 — Product TTL

Products should expire automatically after 60 seconds.

Verify with:

```redis
TTL product:1001
```

---

## Exercise 3 — Product TTL Endpoint

Implement:

```text
GET /api/products/{id}/ttl
```

Expected response:

```json
{
  "productId": 1001,
  "ttlSeconds": 47
}
```

---

## Exercise 4 — Product Existence

Implement:

```text
GET /api/products/{id}/exists
```

---

## Exercise 5 — Bulk Retrieval

Implement:

```text
POST /api/products/bulk
```

Input:

```json
[
  1001,
  1002,
  1003
]
```

Retrieve the Redis values using a multi-key operation where appropriate instead of three sequential GET operations.

---

## Exercise 6 — Redis Inspection

Create several products and inspect:

```redis
KEYS *
SCAN 0
MEMORY USAGE product:1001
TTL product:1001
INFO memory
TYPE product:1001
```

---

## Exercise 7 — Expiration

Create a product with a short TTL:

```text
10 seconds
```

Observe:

```text
Key exists
     ↓
TTL decreases
     ↓
TTL reaches zero
     ↓
Key disappears
```

---

## Exercise 8 — Failure

Stop Redis:

```bash
docker stop redis-playground
```

Call your API.

Observe the behavior.

Restart:

```bash
docker start redis-playground
```

This exercise is intentionally simple; proper production resilience is covered later.

---

# 38. What We Learned

At the end of Stage 0, the core mental model should be:

```text
                    Redis
                      │
             ┌────────┴────────┐
             │                 │
          Keys/Values     Data Structures
             │
             ├── String
             │
             └── TTL
```

From .NET:

```text
ASP.NET Core
      │
      ▼
IConnectionMultiplexer
      │
      ▼
IDatabase
      │
      ▼
StackExchange.Redis
      │
      ▼
Redis
```

For structured application objects:

```text
C# Object
    ↓
Serialization
    ↓
JSON/String
    ↓
Redis
```

---

# 39. Production Lessons From Stage 0

Even at this beginner stage, several production principles have emerged.

### 1. Reuse the Redis connection infrastructure

Use a singleton `IConnectionMultiplexer`.

### 2. Design keys deliberately

Don't create arbitrary key names throughout the application.

### 3. Use TTL for temporary data

Especially for:

- cache entries
- OTPs
- sessions
- temporary locks
- rate-limit state

### 4. Don't use `KEYS *` casually in production

Use `SCAN` for incremental keyspace iteration.

### 5. Redis failure behavior depends on the workload

A cache failure may be recoverable.

A distributed-lock failure may require the operation to stop.

### 6. Cache invalidation is an application responsibility

Redis does not automatically know that your SQL data changed.

### 7. Redis memory is a finite resource

Payload size, key count, eviction policies, and serialization choices matter.

### 8. Redis is not automatically a database replacement

Its role depends on the workload and durability requirements.

---

# 40. Stage 0 Completion Checklist

Before moving to Stage 1:

- [ ] Redis running through Docker
- [ ] Redis CLI working
- [ ] `PING` returns `PONG`
- [ ] `SET` / `GET` understood
- [ ] `DEL` understood
- [ ] `EXISTS` understood
- [ ] TTL understood
- [ ] `EXPIRE` understood
- [ ] `PERSIST` understood
- [ ] Redis logical databases understood
- [ ] `IConnectionMultiplexer` understood
- [ ] `IDatabase` understood
- [ ] StackExchange.Redis installed
- [ ] .NET can read/write Redis
- [ ] Product JSON stored in Redis
- [ ] JSON deserialization understood
- [ ] Redis key naming understood
- [ ] `KEYS` vs `SCAN` understood
- [ ] `MGET` understood
- [ ] `MEMORY USAGE` understood
- [ ] Basic Redis failure tested
- [ ] Cache invalidation concept understood
- [ ] Stage 0 exercises completed

---

# 41. What Stage 1 Adds

Stage 1 is a **separate project**:

```text
RedisDataStructuresApi
```

It will cover:

```text
String
  ↓
Counters

Hash
  ↓
User Profiles

List
  ↓
Queues

Set
  ↓
Roles / Membership

Sorted Set
  ↓
Leaderboards
```

The focus changes from:

> "How do I connect to Redis?"

to:

> "Which Redis data structure should I use to solve this problem?"

Later stages will build on this foundation:

```text
Stage 0
Redis Fundamentals
       ↓
Stage 1
Data Structures
       ↓
Stage 2
Caching
       ↓
Stage 3
Production Caching
       ↓
Stage 4
Pub/Sub
       ↓
Stage 5
Redis Streams
       ↓
Stage 6
Distributed Locks
       ↓
Stage 7
Rate Limiting
       ↓
Stage 8
Atomic Operations + Lua
       ↓
Stage 9
Production Architecture
       ↓
Stage 10
Resilience
       ↓
Stage 11
Observability
       ↓
Stage 12
High Availability
       ↓
Stage 13
Security
       ↓
Stage 14
Production E-Commerce Application
```

---

# 42. Final Stage 0 Project Goal

The finished Stage 0 project is intentionally a **learning/reference project**, not a production application.

Its purpose is to give you direct experience with:

```text
Docker
  +
Redis CLI
  +
StackExchange.Redis
  +
ASP.NET Core
  +
Strings
  +
TTL
  +
Serialization
  +
Key Design
  +
Redis Inspection
```

**Stage 1 should remain a separate project** so that the Stage 0 implementation remains a clean historical reference rather than continually accumulating new Redis features.
