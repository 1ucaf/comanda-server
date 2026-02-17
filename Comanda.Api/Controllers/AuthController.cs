using Comanda.Core.DTOs;
using Comanda.Core.Entities;
using Comanda.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comanda.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ComandaDbContext _context;

    public AuthController(ComandaDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var roleValid = Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role);
        if (!roleValid || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Invalid name or role. Role must be Waiter or Cook.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Name == request.Name && u.Role == role, ct);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Role = role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);
        }

        return Ok(new LoginResponse(
            user.Id,
            user.Name,
            user.Role.ToString()
        ));
    }
}
