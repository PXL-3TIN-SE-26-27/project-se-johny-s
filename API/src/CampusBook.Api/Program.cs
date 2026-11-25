using CampusBook.Api.Data;
using CampusBook.Api.Middleware;
using CampusBook.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddDbContext<CampusDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Campus") ?? "Data Source=campusbook.db"));

builder.Services.AddSingleton<PriceCalculator>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<ExportService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StatsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CampusDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseCors();
app.MapControllers();
app.MapGet("/", () => "CampusBook API");

app.Run();
