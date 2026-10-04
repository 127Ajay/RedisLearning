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

    [HttpGet("set")]
    public async Task<IActionResult> Set()
    {
        await _redis.StringSetAsync(
            "name",
            "Ajay",
            TimeSpan.FromSeconds(30));

        return Ok("Value stored in Redis");
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get()
    {
        var value = await _redis.StringGetAsync("name");

        return Ok(value.ToString());
    }

    [HttpPost("set/{key}/{value}")]
    public async Task<IActionResult> StringSetAsync(string key, string value)
    {

        await _redis.StringSetAsync(
            key,
            value,
            TimeSpan.FromSeconds(30));

        return Ok("StringSetAsync: Value stored in Redis");
    }

    [HttpGet("get/{key}")]
    public async Task<IActionResult> StringGetAsync(string key)
    {
        var value = await _redis.StringGetAsync(key);

        return Ok(value.ToString());
    }

    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> KeyDeleteAsync(string key)
    {
        var value = await _redis.StringGetDeleteAsync(key);
        return Ok($"KeyDeleteAsync: key:{key} with Value:{value} deleted from Redis");
    }

    [HttpGet("exists/{key}")]
    public async Task<IActionResult> KeyExistsAsync(string key)
    {
        var exists = await _redis.KeyExistsAsync(key);
        return Ok(exists);
    }

    [HttpPost("set-expiration/{key}/{value}/{seconds}")]
    public async Task<IActionResult> SetKeyExpireAsync(string key, string value, int seconds)
    {
        await _redis.StringSetAsync(
            key,
            value,
            TimeSpan.FromSeconds(30));
        return Ok("SetKeyExpireAsync: Value stored in Redis");
    }

    [HttpPost("expire/{key}/{seconds}")]
    public async Task<IActionResult> KeyExpireAsync(string key, int seconds)
    {
        var result = await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(seconds));
        return Ok(result);
    }

    [HttpGet("ttl/{key}")]
    public async Task<IActionResult> KeyTimeToLiveAsync(string key)
    {
        var ttl = await _redis.KeyTimeToLiveAsync(key);
        return Ok(ttl);
    }

}