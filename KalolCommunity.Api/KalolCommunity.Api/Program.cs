using KalolCommunity.Api;
using KalolCommunity.Api.Filters;
using KalolCommunity.Api.Handlers;
using KalolCommunity.Application.Interfaces.Infrastructure;
using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Application.Interfaces.Services;
using KalolCommunity.Application.Services;
using KalolCommunity.Application.Validators;
using KalolCommunity.Infrastructure.Persistence;
using KalolCommunity.Infrastructure.Repositories;
using KalolCommunity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.ApplicationInsights;
using System;
using System.Text;
using FluentValidation;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Application Insights FIRST (before UseSerilog)
    builder.Services.AddApplicationInsightsTelemetry();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"],
            TelemetryConverter.Traces));

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
    builder.Services.AddScoped<IContactUsRepository, ContactUsRepository>();
    builder.Services.AddScoped<IContactUsService, ContactUsService>();
    builder.Services.AddScoped<ICachingService, MemoryCacheService>();
    builder.Services.AddScoped<IBlobService, BlobService>();
    builder.Services.AddScoped<IServiceBusPublisher, ServiceBusPublisher>();
    builder.Services.AddScoped<IEmailService, EmailService>();

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
        });

    // Add CORS Policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            var originsCsv = builder.Configuration["Cors:AllowedOriginsCsv"] ?? "http://localhost:4200";

            var origins = originsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            // Add .AllowCredentials() only if you use cookies.
        });
    });

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<AsyncValidationFilter>();
    });

    // Scan the KalolCommunity.Application assembly and automatically register all FluentValidation Validators.
    // Only one validator type is needed as a reference point for assembly scanning.
    // Any new validator (e.g., ForgotPasswordRequestValidator, RegisterUserValidator)
    // added to the same KalolCommunity.Application assembly will be discovered automatically.
    // No additional registration in Program.cs is required.
    builder.Services.AddValidatorsFromAssemblyContaining<SendOtpRequestValidator>();
    
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
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Use CORS - must be before UseAuthentication and UseAuthorization
    app.UseCors("AllowAngularApp");

    app.UseAuthentication();
    app.UseAuthorization();

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
