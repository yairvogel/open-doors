using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Service;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.DbOperations;
using OpenDoors.Service.Interfaces;
using OpenDoors.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext();

builder.Services.AddIdentityCore<TenantUser>()
    .AddRoles<TenantRole>()
    .AddEntityFrameworkStores<OpenDoorsContext>()
    .AddApiEndpoints();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddScoped<TenantManager>();
builder.Services.AddSingleton<EntryLogger>();
builder.Services.AddSingleton<IExternalDoorService, ExternalDoorServiceMock>();

builder.Services.AddScoped<IAuthorizationHandler, AuditorAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AllowedEntryAuthorizationHandler>();
builder.Services.AddAuthorizationPolicies();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Use(next => async context =>
{
    try
    {
        await next(context);
    }
    catch (Exception e)
    {
        context.Response.StatusCode = e is ArgumentException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(e.Message);
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
