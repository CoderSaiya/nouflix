using Microsoft.EntityFrameworkCore;
using MoviePortal.Adapters;
using MoviePortal.Components;
using MoviePortal.Data;
using MoviePortal.Models.Specification;
using MoviePortal.Repositories;
using MoviePortal.Repositories.Interfaces;
using MoviePortal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// EF Core (DbContext pooling + resiliency)
builder.Services.AddDbContextPool<AppDbContext>(opt =>
{
    var cs = builder.Configuration["ConnectionStrings:DefaultConnection"];
    opt.UseSqlServer(cs, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
        sql.CommandTimeout(30);
    });
});

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<FfmpegOptions>(builder.Configuration.GetSection("Ffmpeg"));

// Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IEpisodeRepository, EpisodeRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IStudioRepository, StudioRepository>();
builder.Services.AddScoped<IImageAssetRepository, ImageAssetRepository>();
builder.Services.AddScoped<IVideoAssetRepository, VideoAssetRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Adapter
builder.Services.AddScoped<FfmpegHlsTranscoder>();

// Services
builder.Services.AddSingleton<MinioObjectStorage>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddScoped<MoviesService>();
builder.Services.AddScoped<EpisodesService>();
builder.Services.AddScoped<TaxonomyService>();
builder.Services.AddScoped<EpisodeCsvService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<BulkEpisodesService>();
builder.Services.AddScoped<SystemHealthService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddServerSideBlazor()
    .AddCircuitOptions(o =>
    {
        o.DetailedErrors = true;
    })
    .AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 1024 * 1024 * 1024; // 1GB
        o.ClientTimeoutInterval = TimeSpan.FromMinutes(15);
        o.HandshakeTimeout = TimeSpan.FromSeconds(60);
        o.KeepAliveInterval = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();