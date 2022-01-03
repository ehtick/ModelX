﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using ModelX.Domain.Entities;
using ModelX.Infrastructure.Database;
using ModelX.Logic.Common.AppConfigs.Main;

namespace ModelX.Common.Extensions;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddAutoGeneratedApi(
        this IServiceCollection services)
    {
        services.AddOpenApiDocument(configure =>
        {
            configure.Title = "ModelX Api";
            configure.AddSecurity("JWT", Enumerable.Empty<string>(),
                new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header
                });

            configure.OperationProcessors.Add(
                new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        return services;
    }

    public static IServiceCollection AddAspIdentityJwt(
        this IServiceCollection services
        , IConfiguration configuration)
    {
        var passwordIdentitySettings = configuration
            .GetSection("PasswordIdentitySettings")
            .Get<PasswordIdentitySettings>();

        services.AddIdentity<AppUser, IdentityRole<int>>(options =>
            {
                options.Password.RequiredLength =
                    passwordIdentitySettings.RequiredLength;
                options.Password.RequireNonAlphanumeric =
                    passwordIdentitySettings.RequireNonAlphanumeric;
                options.Password.RequireLowercase =
                    passwordIdentitySettings.RequireLowercase;
                options.Password.RequireUppercase =
                    passwordIdentitySettings.RequireUppercase;
                options.Password.RequireDigit =
                    passwordIdentitySettings.RequireDigit;
                options.User.RequireUniqueEmail = true;
                options.Tokens.PasswordResetTokenProvider =
                    TokenOptions.DefaultPhoneProvider;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserStore<UserStore<AppUser, IdentityRole<int>
                , AppDbContext, int>>()
            .AddRoleStore<RoleStore<IdentityRole<int>
                , AppDbContext, int>>()
            .AddDefaultTokenProviders();

        var authOptions = configuration
            .GetSection(nameof(AuthOptions))
            .Get<AuthOptions>();

        services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata =
                    authOptions.RequireHttpsMetadata;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = authOptions.SymmetricSecurityKey,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}