using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;
using OpenDoors.Service.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OpenDoorsContext>(options =>
{
    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    string dbPath = Path.Join(localAppData, "opendoors.db");
    options.UseInMemoryDatabase("opendoors"); // ($"Data Source={dbPath}");
});

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<TenantUser>()
    .AddEntityFrameworkStores<OpenDoorsContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<TenantManager>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapPost("/doors", async (Door door, OpenDoorsContext dbContext) =>
{
    await dbContext.AddAsync(door);
    await dbContext.SaveChangesAsync();
})
.RequireAuthorization()
.WithName("Create Door")
.WithOpenApi();

app.MapGet("/doors", (OpenDoorsContext dbContext) => dbContext.Doors.ToListAsync())
.RequireAuthorization()
.WithName("Get Doors")
.WithOpenApi();

app.Run();
