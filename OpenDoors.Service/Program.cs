using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OpenDoorsContext>(options =>
{
    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    string dbPath = Path.Join(localAppData, "opendoors.db");
    options.UseInMemoryDatabase("opendoors"); // ($"Data Source={dbPath}");
});

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<OpenDoorsContext>()
    .AddApiEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapIdentityApi<IdentityUser>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
