using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model;
using OpenDoors.Service.Authorization;
using OpenDoors.Service.DbOperations;
using OpenDoors.Service.Services;

namespace OpenDoors.Service.Authentication;

[ApiController]
public class AuthenticationController(SignInManager<TenantUser> signInManager, UserManager<TenantUser> userManager, TenantManager tenantManager, OpenDoorsContext dbContext) : ControllerBase
{
    private static readonly EmailAddressAttribute _emailValidator = new();

    [HttpPost]
    [Route("/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        string email = registerRequest.Email;

        if (string.IsNullOrEmpty(email) || !_emailValidator.IsValid(email))
        {
            return BadRequest($"email {email} is not a valid email address");
        }

        if (string.IsNullOrEmpty(registerRequest.Tenant))
        {
            return BadRequest("tenant is required");
        }

        Tenant? tenant = await tenantManager.GetByNameAsync(registerRequest.Tenant);
        if (tenant == null)
        {
            tenant = await tenantManager.CreateAsync(new Tenant { Name = registerRequest.Tenant });
        }

        TenantUser user = new TenantUser { Email = email, Tenant = tenant, UserName = email };

        IdentityResult result = await userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.ToList());
        }

        AccessGroup defaultAccessGroup = await dbContext.GetDefaultAccessGroup(tenant.Id!.Value);

        await dbContext.AddUserToAccessGroup(user.Id, defaultAccessGroup);
        await userManager.AddClaimAsync(user, new Claim(AuthorizationConstants.TenantClaimType, tenant.Id.ToString()!));
        if (registerRequest.Admin)
        {
            await userManager.AddToRoleAsync(user, AuthorizationConstants.AdminRole);
        }

        if (registerRequest.Auditor)
        {
            await userManager.AddToRoleAsync(user, AuthorizationConstants.AuditorRole);
        }

        return Ok();
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

        var result = await signInManager.PasswordSignInAsync(loginRequest.Email, loginRequest.Password, isPersistent: true, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        return Empty;
    }

    [HttpGet("/user")]
    [Authorize(AuthorizationConstants.HasTenantPolicy)]
    public IActionResult GetCurrentUser()
    {
        Guid tenantId = HttpContext.User.GetTenantId();
        string userId = HttpContext.User.GetUserId();
        return Ok(new CurrentUserDto(userId, HttpContext.User.FindFirst(ClaimTypes.Email)!.Value, tenantId.ToString()));
    }
}

public record RegisterRequest(string Email, string Password, string Tenant, bool Admin = false, bool Auditor = false);
public record LoginRequest(string Email, string Password);
public record CurrentUserDto(string UserId, string Email, string Tenant);
