using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly List<User> _users;
    private readonly string _jwtKey ;

    public AuthController(List<User> users, IConfiguration config)
    {
        _users = users;
        _jwtKey = config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(_jwtKey))
            throw new ArgumentException("JWT key is missing in configuration (Jwt:Key)");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, "user")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new { token = tokenString });
    }
}

public class LoginRequest
{
    public string Email { get; set; }
}
