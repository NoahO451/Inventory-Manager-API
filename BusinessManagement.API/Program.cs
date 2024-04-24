using App.Middlewares;
using App.Models.Validators;
using App.Repositories;
using App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Templates;
using Serilog.Templates.Themes;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.WithExceptionDetails()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new ExpressionTemplate(
            "[{@t:HH:mm:ss} {@l:u3}{#if RequestId is not null} {RequestId}{#end}{#if ClientIpAddress is not null} {ClientIpAddress }{#end}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
            theme: TemplateTheme.Literate)));

    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.Listen(IPAddress.Any, 6060, listenOptions =>
        {
            listenOptions.UseHttps();
        });
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
        Log.Fatal($"Configure auth0 properties in appsettings.json {auth0Settings}. Check secrets.", LogHelper.TraceLog());
        throw new Exception($"Configure auth0 properties in appsettings.json {auth0Settings}.");
    }

    builder.Services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // ignore omitted parameters on models to enable optional params (e.g. User update)
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

    builder.Host.ConfigureServices((services) =>
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                var audience = builder.Configuration.GetValue<string>("Auth0Settings:AUTH0_AUDIENCE");

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

    // Custom policy provider
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();

    // Register the custom authorization handler
    builder.Services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("UpdateUsers", policy =>
                  policy.RequireClaim("permissions", "update:users"));
    });

    builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

    // configure strongly typed settings object
    builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));

    builder.Services.AddSwaggerGen(options =>
    {
        var domain = builder.Configuration.GetValue<string>("Auth0Settings:AUTH0_DOMAIN");
        var audience = builder.Configuration.GetValue<string>("Auth0Settings:AUTH0_AUDIENCE");

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "API Documentation",
            Version = "v1.0",
            Description = ""
        });

        options.ResolveConflictingActions(x => x.First());
        //options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        //{
        //    Type = SecuritySchemeType.OAuth2,
        //    BearerFormat = "JWT",
        //    Flows = new OpenApiOAuthFlows
        //    {
        //        Implicit = new OpenApiOAuthFlow
        //        {

        //            TokenUrl = new Uri($"https://{domain}/oauth/token"),
        //            AuthorizationUrl = new Uri($"https://{domain}/authorize?audience={audience}"),
        //            Scopes = new Dictionary<string, string>
        //            {
        //                { "openid", "OpenId" }
        //            }
        //        }
        //    }
        //});
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            BearerFormat = "JWT",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://{domain}/authorize?audience={audience}"),
                    TokenUrl = new Uri($"https://{domain}/oauth/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect" },
                        { "profile", "User profile information" }
                    }
                }
            }
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
              new OpenApiSecurityScheme
              {
                  Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
              },
              new[] { "openid" }
          }
      });

        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    });


    // configure DI for application services
    builder.Services.AddSingleton<DataContext>();
    builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuth0Service, Auth0Service>();
    builder.Services.AddScoped<IUserRolePermissionRepository, UserRolePermissionRepository>();
    

    builder.Services.AddValidatorsFromAssemblyContaining<AddInventoryItemRequestValidator>();

    builder.Services.AddHttpClient("Auth0Domain", httpClient =>
    {
        httpClient.BaseAddress = new Uri($"https://{auth0Settings.AUTH0_DOMAIN}");
    });

    var app = builder.Build();

    // Commented for later, don't need this yet
    //app.UseHealthAndMetricsMiddleware();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "{RemoteIpAddress} {RequestScheme} {RequestMethod} {RequestHost}{RequestPath}{Query} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;

        options.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest;
    });

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
            Log.Fatal($"Config variable missing: {key}. Check secrets.", LogHelper.TraceLog());
            throw new Exception($"Config variable missing: {key}.");
        }
    }

    app.Urls.Add(
        $"http://+:{app.Configuration.GetValue<string>("Auth0Settings:PORT")}");

    app.UseSwagger();
    app.UseSwaggerUI(settings =>
    {
        string? clientId = app.Configuration.GetValue<string>("ClientId");
        string? clientSecret = app.Configuration.GetValue<string>("ClientSecret");

        settings.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0");
        settings.OAuthClientId(clientId);
        settings.OAuthClientSecret(clientSecret);
        settings.OAuthUsePkce();
    });

    app.UseErrorHandler();
    app.UseSecureHeaders();
    app.MapControllers();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}