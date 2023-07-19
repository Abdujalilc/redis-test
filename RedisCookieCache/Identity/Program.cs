using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using RedisCookieCache.RedisService;
using RedisCookieCache.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "127.0.0.1:6379";
});

builder.Services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>());
builder.Services.AddSingleton<ITicketStore, RedisCacheTicketStore>();
builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();

builder.Services.AddAuthentication("CookieAuthentication")
                 .AddCookie("CookieAuthentication", config =>
                 {
                     config.Cookie.Name = "UserLoginCookie";
                     config.LoginPath = "/Account/LoginMain";
                     config.AccessDeniedPath = "/Account/UserAccessDenied";
                 });

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, store) => { options.SessionStore = store; });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/protected", () => "From Protected Project!!!").RequireAuthorization();
app.MapGet("/login", (HttpContext ctx) =>
{
    var check = ctx.User.Identity.IsAuthenticated;
    if (!check)
    {
        ctx.SignInAsync(
            new ClaimsPrincipal(new ClaimsIdentity[] {
                new ClaimsIdentity(claims:new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
                },
                authenticationType:CookieAuthenticationDefaults.AuthenticationScheme)
            }));
    }
    return "ok";
}
);

app.Run();
