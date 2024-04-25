using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenDoors.Model.Authentication;

namespace OpenDoors.Service.Controllers;

[ApiController]
public class AuthenticationController(SignInManager<TenantUser> signInManager, UserManager<TenantUser> userManager, TenantManager tenantManager) : ControllerBase
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

        Tenant? tenant = await tenantManager.GetByNameAsync(registerRequest.Tenant);
        if (tenant is null)
        {
            tenant = await tenantManager.CreateAsync(new Tenant { Name = registerRequest.Tenant });
        }

        TenantUser user = new TenantUser
        {
            Email = email,
            Tenant = tenant,
            UserName = email
        };

        IdentityResult result = await userManager.CreateAsync(user, registerRequest.Password);


        return result.Succeeded ? Ok() : BadRequest(result.Errors.ToList());
    }

    [HttpPost]
    [Route("/login")]
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
}

public record RegisterRequest(string Email, string Password, string Tenant);

public record LoginRequest(string Email, string Password);
