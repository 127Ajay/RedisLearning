using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisPlayground.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly IDatabase _redis;

    public RedisController(IConnectionMultiplexer connectionMultiplexer)
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
        var value = await _redis.StringGetAsync(key);

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

    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> KeyDeleteAsync(string key)
    {
        var deleted = await _redis.KeyDeleteAsync(key);
        return Ok(new
        {
            Key = key,
            Deleted = deleted
        });
    }

    [HttpGet("exists/{key}")]
    public async Task<IActionResult> KeyExistsAsync(string key)
    {
        var exists = await _redis.KeyExistsAsync(key);
        return Ok(new
        {
            Key = key,
            Exists = exists
        });
    }

    [HttpPost("set-expiration/{key}/{value}/{seconds}")]
    public async Task<IActionResult> SetKeyExpireAsync(string key, string value, int seconds)
    {
        await _redis.StringSetAsync(
            key,
            value,
            TimeSpan.FromSeconds(seconds));

        return Ok(new
        {
            Key = key,
            Value = value,
            ExpirationSeconds = seconds
        });
    }

    [HttpPost("expire/{key}/{seconds}")]
    public async Task<IActionResult> KeyExpireAsync(string key, int seconds)
    {
        var exists = await _redis.KeyExistsAsync(key);

        if (!exists)
        {
            return NotFound();
        }

        await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(seconds));
        return Ok(new
        {
            Key = key,
            ExpirationSeconds = seconds
        });
    }

    [HttpGet("ttl/{key}")]
    public async Task<IActionResult> KeyTimeToLiveAsync(string key)
    {
        var ttl = await _redis.KeyTimeToLiveAsync(key);
        return Ok(new
        {
            Key = key,
            Ttl = ttl?.TotalSeconds
        });
    }

}