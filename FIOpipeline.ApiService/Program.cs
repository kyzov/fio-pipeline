using FIOpipeline.Core;
using FIOpipeline.Core.DataAccess;
using FIOpipeline.Core.Providers;
using FIOpipeline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IPersonProvider, PersonProvider>();
builder.Services.AddScoped<IDeduplicationProvider, DeduplicationProvider>();
builder.Services.AddScoped<IShowcaseProvider, ShowcaseProvider>();
builder.Services.AddScoped<ITemporalDataService, TemporalDataService>();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.InitializeTemporalData();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseRouting();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
