using MoviePortal.Api;
using MoviePortal.Components;

var builder = WebApplication.CreateBuilder(args);

// builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

var baseUrl = builder.Configuration["BaseUrl:Backend"]!;
builder.Services.AddHttpClient<TranscodeApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddHttpClient<MovieApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddHttpClient<TaxonomyApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddHttpClient<SubtitlesApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<StorageApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<HealthApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<CsvApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<BulkEpisodeApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<DashboardApi>(c =>
{
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddScoped<JobPoller>();

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