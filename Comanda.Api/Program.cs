using Comanda.Api.Hubs;
using Comanda.Infrastructure;
using Comanda.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ComandaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration["DATABASE_URL"];
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection o DATABASE_URL");
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsAssembly("Comanda.Infrastructure"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var corsOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ComandaDbContext>();
    await DbSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrdersHub>("/hubs/orders");
app.MapGet("/healthz", () => Results.Ok()).AllowAnonymous();

app.Run();
