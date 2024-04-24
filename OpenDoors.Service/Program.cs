using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OpenDoorsContext>(options =>
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Join(localAppData, "opendoors.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/users", async (User user, OpenDoorsContext dbContext) =>
{
    await dbContext.AddAsync(user);
    await dbContext.SaveChangesAsync();
})
.WithName("Create User")
.WithOpenApi();

app.MapGet("/users", (OpenDoorsContext dbContext) => dbContext.Users.ToListAsync())
.WithName("Get Users")
.WithOpenApi();

app.Run();
