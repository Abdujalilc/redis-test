using IdentityApp.Services;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect("127.0.0.1:6379"))
    .SetApplicationName("some_unique_name");
/*builder.Services.AddSingleton<ITicketStore, RedisCacheTicketStore>();
builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, store) =>
    {
        options.SessionStore = store;
    });*/

builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme);
builder.Services.ConfigureApplicationCookie(opts =>
    opts.SessionStore = new RedisCacheTicketStore(new RedisCacheOptions()
    {
        Configuration = "127.0.0.1:6379"
    })
    );


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "YourAppCookieName";
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                if (claimsIdentity?.IsAuthenticated == true)
                {
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {

                    }
                }
            }
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/protected", () => "Secret from Identity project!!!").RequireAuthorization();
app.MapGet("/login", (HttpContext ctx) =>
{
    var check = ctx.User.Identity.IsAuthenticated;
    if (!check)
    {
        ctx.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity[]
                {
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
