using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;
using OpenDoors.Service;
using OpenDoors.Service.Handlers;

namespace OpenDoors.Tests;

public class DoorHandlerTests
{
    private readonly OpenDoorsContext _dbContext;
    private readonly DoorHandler _sut;

    private readonly Tenant _tenant;
    private readonly TenantUser _user;

    public DoorHandlerTests()
    {
        var dbOptions = new DbContextOptionsBuilder<OpenDoorsContext>()
            .UseInMemoryDatabase(databaseName: nameof(DoorHandlerTests))
            .Options;
        _dbContext = new OpenDoorsContext(dbOptions);

        _tenant = new Tenant { Name = nameof(DoorHandlerTests) };
        _dbContext.Add(_tenant);
        _user = new TenantUser { Tenant = _tenant, Email = "a@g.com", UserName = "a@g.com" };
        _dbContext.Add(_user);
        _dbContext.SaveChanges();

        _sut = new DoorHandler(_dbContext);
    }

    [Fact]
    public async Task ShouldCreateADoor()
    {
        await _sut.CreateDoor("Door", _tenant.Id!.Value!);

        var door = _dbContext.Doors.Where(d => d.Tenant.Id == _tenant.Id).FirstOrDefault();
        door.Should().NotBeNull();
        door!.Location.Should().Be("Door");
    }

    [Fact]
    public async Task ShouldListAvailableDoors()
    {
        await _sut.CreateDoor("Door", _tenant.Id!.Value!);
        await _sut.CreateDoor("Door2", _tenant.Id!.Value!);

        IReadOnlyList<Door> doors = await _sut.ListDoorsForUser(_user.Id!, _tenant.Id.Value);

        doors.Should().HaveCount(2);
        doors.Should().Contain(d => d.Location == "Door");
        doors.Should().Contain(d => d.Location == "Door2");
    }
}
