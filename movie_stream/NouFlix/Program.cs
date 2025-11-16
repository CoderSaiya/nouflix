using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using NouFlix.Configuration;
using NouFlix.Models.Specification;
using Serilog;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth:Auth"));
builder.Services.Configure<AuthCookieOptions>(builder.Configuration.GetSection("Auth:Cookie"));
builder.Services.Configure<FfmpegOptions>(builder.Configuration.GetSection("Ffmpeg"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddSwagger(builder.Configuration);

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(8083, listenOptions =>
//     {
//         listenOptions.UseHttps("/https/nouflix.pfx", "123456");
//     });
// });

var app = builder.Build();

var fwd = new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
    RequireHeaderSymmetry = false,
    ForwardLimit = null
};

fwd.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.23.0.0"), 16));
app.UseForwardedHeaders(fwd);

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddlewares();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();