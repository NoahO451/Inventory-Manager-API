using App.Helpers;
using App.Middlewares;
using App.Models;
using App.Repositories;
using App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Configuration.GetValue<string>("Auth0Settings:CLIENT_ORIGIN_URL"))
            .WithHeaders(new string[] {
                HeaderNames.ContentType,
                HeaderNames.Authorization,
            })
            .WithMethods("GET")
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});

Auth0Settings? auth0Settings = builder.Configuration.GetSection("Auth0Settings").Get<Auth0Settings>();

if (auth0Settings == null)
{
    throw new Exception($"Configure auth0 properties in appsettings.json {auth0Settings}.");
}

builder.Services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

    // ignore omitted parameters on models to enable optional params (e.g. User update)
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Host.ConfigureServices((services) =>
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var audience =
                  builder.Configuration.GetValue<string>("Auth0Settings:AUTH0_AUDIENCE");

            options.Authority =
                  $"https://{builder.Configuration.GetValue<string>("Auth0Settings:AUTH0_DOMAIN")}/";
            options.Audience = audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuerSigningKey = true
            };
        })
);

// configure strongly typed settings object
builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));

// configure DI for application services
builder.Services.AddSingleton<DataContext>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuth0Service, Auth0Service>();

builder.Services.AddHttpClient("Auth0Domain", httpClient =>
{
    httpClient.BaseAddress = new Uri($"https://{auth0Settings.AUTH0_DOMAIN}");
});

var app = builder.Build();

// ensure database and tables exist
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    await context.Init();
}

var requiredVars =
    new string[] {
          "PORT",
          "CLIENT_ORIGIN_URL",
          "AUTH0_DOMAIN",
          "AUTH0_AUDIENCE",
    };

foreach (var key in requiredVars)
{
    var value = app.Configuration.GetValue<string>($"Auth0Settings:{key}");

    if (value == "" || value == null)
    {
        throw new Exception($"Config variable missing: {key}.");
    }
}

app.Urls.Add(
    $"http://+:{app.Configuration.GetValue<string>("Auth0Settings:PORT")}");

app.UseErrorHandler();
app.UseSecureHeaders();
app.MapControllers();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
