//SimpleApp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect("127.0.0.1:6379"))
    .SetApplicationName("some_unique_name");

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
/*
 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o=>
    {
        o.Cookie.Domain = ".wiut.uz";
    });
 */

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/protected", () => "Secret from application project!!!").RequireAuthorization();

app.MapGet("/login", (HttpContext ctx) =>
{
    ctx.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity[]
    {
            new ClaimsIdentity(claims:new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
            },
            authenticationType:CookieAuthenticationDefaults.AuthenticationScheme)
    }));
    return "ok";
}
);

app.Run();