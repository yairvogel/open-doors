using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OpenDoors.Model;
using OpenDoors.Model.Authentication;
using OpenDoors.Service;
using OpenDoors.Service.Handlers;
using OpenDoors.Service.Interfaces;

namespace OpenDoors.Tests;

public class DoorHandlerTests
{
    private readonly OpenDoorsContext _dbContext;

    private readonly DoorHandler _sut;

    private readonly Tenant _tenant;

    private readonly TenantUser _user;

    private readonly AccessGroup _accessGroup;

    private readonly Mock<ITenantManager> _tenantManagerMock;

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
        _accessGroup = new AccessGroup { Tenant = _tenant, Name = "default", Members = [_user], Doors = [] };
        _dbContext.Add(_accessGroup);
        _dbContext.SaveChanges();

        _tenantManagerMock = new(MockBehavior.Strict);
        _tenantManagerMock.Setup(t => t.GetDefaultAccessGroup(_tenant.Id!.Value!, It.IsAny<CancellationToken>())).ReturnsAsync(_accessGroup);

        _sut = new DoorHandler(_dbContext, _tenantManagerMock.Object);
    }

    [Fact]
    public async Task ShouldCreateADoor()
    {
        await _sut.CreateDoor("Door", _tenant.Id!.Value!);

        var door = _dbContext.Doors.Where(d => d.Location == "Door" && d.AccessGroups.Contains(_accessGroup)).FirstOrDefault();
        door.Should().NotBeNull();
        door!.Location.Should().Be("Door");
        door!.AccessGroups.Should().BeEquivalentTo([_accessGroup]);
    }

    [Fact]
    public async Task ShouldListAvailableDoors()
    {
        await _sut.CreateDoor("Door", _tenant.Id!.Value!);
        await _sut.CreateDoor("Door2", _tenant.Id!.Value!);

        IReadOnlyList<Door> doors = await _sut.ListDoorsForUser(_user.Id!);

        doors.Should().HaveCount(2);
        doors.Should().Contain(d => d.Location == "Door");
        doors.Should().Contain(d => d.Location == "Door2");
    }
}
