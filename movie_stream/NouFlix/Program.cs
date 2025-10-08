using NouFlix.Configuration;
using NouFlix.Models.Specification;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth:Auth"));
builder.Services.Configure<AuthCookieOptions>(builder.Configuration.GetSection("Auth:Cookie"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddSwagger(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();