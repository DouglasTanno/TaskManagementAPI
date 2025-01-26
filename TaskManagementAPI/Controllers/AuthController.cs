using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Services;
using TaskManagementAPI.Services.Implementations;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;

    public AuthController(IConfiguration configuration, IUserService userService)
    {
        _configuration = configuration;
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {        var user = _userService.ValidateUser(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        var secretKey = _configuration["JwtSettings:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        string tokenString = tokenHandler.WriteToken(token);

        return Ok(new JwtResponse { Token = tokenString });
    }

    [HttpPost("register")]
    [Authorize]
    public IActionResult Register([FromBody] UserCreateDTO userCreateDTO)
    {
        if (string.IsNullOrEmpty(userCreateDTO.Username) || string.IsNullOrEmpty(userCreateDTO.Password))
        {
            return BadRequest("Username and Password are required.");
        }

        try
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var authenticatedUserId))
            {
                return Unauthorized();
            }

            _userService.RegisterUser(userCreateDTO, authenticatedUserId);
            return Ok("Usuário cadastrado com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class JwtResponse
{
    public string Token { get; set; }
}
