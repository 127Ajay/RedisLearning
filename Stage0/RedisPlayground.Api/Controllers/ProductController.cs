using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisPlayground.Api.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisPlayground.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IDatabase _redis;

        public ProductController(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer.GetDatabase();
        }

        [HttpPost]
        public async Task<IActionResult> SetProduct(Product product)
        {
            var key = $"product:{product.Id}";

            var json = JsonSerializer.Serialize(product);

            await _redis.StringSetAsync(
                            key,
                            json,
                            TimeSpan.FromSeconds(60));

            return Ok(new
            {
                Key = key,
                Product = product
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var key = $"product:{id}";

            var json = await _redis.StringGetAsync(key);

            if (json.IsNullOrEmpty)
            {
                return NotFound();
            }

            var product =
                JsonSerializer.Deserialize<Product>(json.ToString());

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var key = $"product:{id}";

            var deleted = await _redis.KeyDeleteAsync(key);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id:int}/ttl")]
        public async Task<ActionResult> GetProductTTL(int id)
        {
            var key = $"product:{id}";
            var ttl = await _redis.KeyTimeToLiveAsync(key);
            if (ttl is null)
            {
                return NotFound();
            }
            return Ok(new
            {
                Key = key,
                TTL = ttl.Value.TotalSeconds
            });
        }

        [HttpPost("ttl")]
        public async Task<ActionResult> SetProductTTL([FromBody] ProductTTL requst)
        {
            var key = $"product:{requst.Id}";

            bool wasSet = await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(requst.Seconds));

            if (!wasSet)
            {
                return NotFound("Key does not exist in Redis.");
            }

            return Ok(new { Key = key, TTLSeconds = requst.Seconds });
        }

        [HttpGet("{id:int}/exists")]
        public async Task<ActionResult> KeyExists(int id)
        {
            var key = $"product:{id}";
            bool exists = await _redis.KeyExistsAsync(key);

            return Ok(new
            {
                Key = key,
                Exists = exists
            });
        }

        [HttpPost("Bulk")]
        public async Task<IActionResult> GetProductsBulk([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Product IDs list cannot be empty.");
            }

            // 1. Map the integer IDs to Redis keys
            RedisKey[] keys = ids.Select(id => (RedisKey)$"product:{id}").ToArray();

            // 2. Fetch all keys in a single bulk operation (MGET)
            RedisValue[] values = await _redis.StringGetAsync(keys);

            // 3. Combine the IDs and their corresponding values for the response
            var results = ids.Select((id, index) => new
            {
                Id = id,
                Key = keys[index].ToString(),
                // If the value has data, parse/return it; otherwise null
                Data = values[index].HasValue ? values[index].ToString() : null,
                Exists = values[index].HasValue
            });

            return Ok(results);
        }
    }
}
