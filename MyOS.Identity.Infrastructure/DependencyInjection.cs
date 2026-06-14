using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Application.Extensions;
using MyOS.Identity.Domain.Users;
using MyOS.Identity.Infrastructure.EntityConfigurations.Users;
using MyOS.Identity.Infrastructure.Repositories;
using MyOS.Identity.Infrastructure.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MyOS.Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEfConfigurationsFromAssembly(typeof(UserEntityConfiguration).Assembly);

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            await WriteAuthProblemDetails(context.HttpContext, UserErrors.Unauthorized);
                        },
                        OnForbidden = async context =>
                        {
                            await WriteAuthProblemDetails(context.HttpContext, UserErrors.Forbidden);
                        }
                    };
                });

            services.AddIdentityApplication();

            return services;
        }

        private static async Task WriteAuthProblemDetails(HttpContext httpContext, Error error)
        {
            var status = error.Type == ErrorType.Forbidden
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status401Unauthorized;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = error.Type.ToString(),
                Detail = error.Message,
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            problem.Extensions["correlationId"] = httpContext.TraceIdentifier;
            problem.Extensions["errorCode"] = error.Code;

            httpContext.Response.StatusCode = status;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}
