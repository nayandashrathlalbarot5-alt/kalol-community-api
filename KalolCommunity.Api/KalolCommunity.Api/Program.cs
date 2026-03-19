using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using KalolCommunity.Application.Services;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Infrastructure.Services;
using KalolCommunity.Infrastructure.Persistence;
using KalolCommunity.Infrastructure.Repositories;
using KalolCommunity.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using KalolCommunity.Api.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.ApplicationInsights;


try
{
    var builder = WebApplication.CreateBuilder(args);

    var appInsightsConnectionString =
        builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? builder.Configuration["ApplicationInsights:ConnStr"]
        ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

    if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
    {
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
    }

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            loggerConfiguration.WriteTo.ApplicationInsights(
                appInsightsConnectionString,
                TelemetryConverter.Traces);
        }
    });

    // Add services to the container.
    builder.Services.AddDbContext<KalolCommunityDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions
                .EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)
                .CommandTimeout(60)
        )
    );

    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICountryRepository, CountryRepository>();
    builder.Services.AddScoped<IStateRepository, StateRepository>();
    builder.Services.AddScoped<IMasterDataService, MasterDataService>();
    builder.Services.AddScoped<ICommunityDetailService, CommunityDetailService>();
    builder.Services.AddScoped<ICachingService, MemoryCacheService>();
    builder.Services.AddScoped<IBlobService, BlobService>();
    builder.Services.AddScoped<IServiceBusPublisher, ServiceBusPublisher>();
    
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

    
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ClockSkew = TimeSpan.Zero

            };

            //options.Events = new JwtBearerEvents
            //{
            //    OnAuthenticationFailed = context =>
            //    {
            //        var logger = context.HttpContext.RequestServices
            //            .GetRequiredService<ILoggerFactory>()
            //            .CreateLogger("JwtBearerEvents");

            //        logger.LogWarning(
            //            context.Exception,
            //            "JWT authentication failed for {Path}. Token may be invalid/expired/signature mismatch.",
            //            context.HttpContext.Request.Path);

            //        return Task.CompletedTask;
            //    },
            //    OnTokenValidated = context =>
            //    {
            //        var logger = context.HttpContext.RequestServices
            //            .GetRequiredService<ILoggerFactory>()
            //            .CreateLogger("JwtBearerEvents");

            //        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //        logger.LogInformation(
            //            "JWT token validated successfully for {Path}. UserId: {UserId}",
            //            context.HttpContext.Request.Path,
            //            userId);

            //        return Task.CompletedTask;
            //    },
            //    OnChallenge = context =>
            //    {
            //        var logger = context.HttpContext.RequestServices
            //            .GetRequiredService<ILoggerFactory>()
            //            .CreateLogger("JwtBearerEvents");

            //        logger.LogWarning(
            //            "JWT challenge triggered for {Path}. Error: {Error}, Description: {Description}",
            //            context.HttpContext.Request.Path,
            //            context.Error,
            //            context.ErrorDescription);

            //        return Task.CompletedTask;
            //    }
            //};
        });

    // Add CORS Policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();

        });
    });

    builder.Services.AddControllers();
    builder.Services.AddMemoryCache();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Kalol Community API",
            Version = "v1"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT token like: Bearer {your_token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "Bearer",
                    Name = "Authorization",
                    In = ParameterLocation.Header
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Uncomment to generate state inserts
    //StateInsertGenerator.GenerateInserts();
    //return; // Exit after generation

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Use CORS - must be before UseAuthentication and UseAuthorization
    app.UseCors("AllowAngularApp");

    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapGet("/api/debug/token", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var claims = user.Claims
                .Select(c => new { c.Type, c.Value })
                .ToArray();

            return Results.Ok(new
            {
                IsAuthenticated = true,
                AuthenticationType = user.Identity?.AuthenticationType,
                Name = user.Identity?.Name,
                Claims = claims
            });
        }).RequireAuthorization();
    }

    app.MapControllers();

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
