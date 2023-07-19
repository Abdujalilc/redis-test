//builder.Services.AddSingleton<ITicketStore, RedisTicketStore>();

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using StackExchange.Redis;
using System.Security.Claims;

public class RedisTicketStore : ITicketStore
{
    private readonly IDatabase _database;
    private readonly string _prefix = "TicketStore-";

    public RedisTicketStore(IDatabase database)
    {
        _database = database;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = $"{_prefix}{Guid.NewGuid()}";
        await RenewAsync(key, ticket);
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var value = TicketSerializer.Default.Serialize(ticket);
        return _database.StringSetAsync(key, value, TimeSpan.FromMinutes(30));
    }

    public Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        var value = _database.StringGet(key);
        return Task.FromResult(value.HasValue ? TicketSerializer.Default.Deserialize(value) : null);
    }

    public Task RemoveAsync(string key)
    {
        return _database.KeyDeleteAsync(key);
    }
}