using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using NouFlix.Adapters;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories;
using NouFlix.Persistence.Repositories.Interfaces;
using NouFlix.Services;
using NouFlix.Services.Interface;

namespace NouFlix.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    connectionString: configuration["ConnectionStrings:Default"],
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);
        
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                // tự động gắn traceId nếu chưa có
                ctx.ProblemDetails.Extensions.TryAdd("traceId",
                    Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier);
            };
        });
        
        services.AddStackExchangeRedisCache(o =>
        {
            o.Configuration = configuration.GetConnectionString("Redis");
            o.InstanceName = "movies:"; // prefix key
        });
        
        services.AddHttpClient("origin").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false
        });
        
        services.AddExceptionHandler<AppExceptionHandler>();
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Repositories
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IStudioRepository, StudioRepository>();
        services.AddScoped<IImageAssetRepository, ImageAssetRepository>();
        services.AddScoped<IVideoAssetRepository, VideoAssetRepository>();
        services.AddScoped<ISeasonRepository, SeasonRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Adapters
        services.AddScoped<AspNetExternalTicketReader>();
        services.AddScoped<AuthUrlBuilder>();
        services.AddScoped<TokenCookieWriter>();
        
        // Services
        services.AddScoped<MinioObjectStorage>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<AccessService>();
        services.AddScoped<StreamService>();
        services.AddScoped<MovieService>();
        services.AddScoped<SeasonService>();
        services.AddScoped<ExternalAuth>();
        
        // App cache wrapper
        services.AddSingleton<IAppCache, DistributedCache>();
        
        return services;
    }
    
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                        logger.LogDebug("JWT Header: {Auth}", authHeader);
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ctx.Exception, "JWT failed");
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            })
            .AddCookie("External");
            // .AddGoogle("Google", options =>
            // {
            //     options.ClientId = configuration["Auth:External:Google:ClientId"]!;
            //     options.ClientSecret = configuration["Auth:External:Google:ClientSecret"]!;
            //     options.CallbackPath = "/signin-google"; // đăng trong Google Console
            //     options.SignInScheme = "External";
            //     options.SaveTokens = true;
            // });
        
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                        "http://localhost:4200",
                        "https://nouflix.nhatcuong.io.vn")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
    
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "MovieStream", Version = "v1" }));

        services.AddSwaggerGen(c =>
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            }));
        services.AddSwaggerGen(c =>
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            }));

        return services;
    }
}