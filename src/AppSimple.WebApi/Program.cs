using AppSimple.Core.Auth;
using AppSimple.DataLib.Db;
using AppSimple.WebApi.Extensions;
using AppSimple.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebApiServices();

var app = builder.Build();

// Seed DB on startup
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    db.Initialize();
    db.SeedAdminUser(hasher.Hash("Admin123!"));
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
