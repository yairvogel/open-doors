using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model.Authentication;
using OpenDoors.Service;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OpenDoorsContext>(options =>
{
    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    string dbPath = Path.Join(localAppData, "opendoors.db");
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddSingleton<IAuthorizationHandler, AllowedEntryHandler>();

builder.Services.AddIdentityCore<TenantUser>()
    .AddRoles<TenantRole>()
    .AddEntityFrameworkStores<OpenDoorsContext>()
    .AddApiEndpoints(); // TODO: do I need this?

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddScoped<DoorRepository>();
builder.Services.AddScoped<DoorHandler>();

builder.Services.AddAuthorizationPolicies();

builder.Services.AddScoped<TenantManager>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
