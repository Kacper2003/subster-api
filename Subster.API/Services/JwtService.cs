using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Subster.API.Services;

public class JwtService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

	public string GenerateToken(string ssn, string name, string role)
    {
		IConfigurationSection jwtSettings = _configuration.GetSection("JwtSettings")
            ?? throw new Exception("JWT settings not configured");

        // Retrieve the secret key from configuration used for signing the token
        var secret = jwtSettings["SecretKey"] ?? throw new Exception("JWT secret key not found");
        var key = Encoding.UTF8.GetBytes(secret);

        var tokenHandler = new JwtSecurityTokenHandler();

		// Set up the claims using the provided parameters
		Claim[] claims =
		[
			new Claim("Ssn", ssn),
            new Claim("Name", name),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var expiresInMinuntes = jwtSettings.GetValue<int>("TokenExpirationMinutes");

        var issuer = jwtSettings["Issuer"] ?? throw new Exception("JWT issuer not found");
        var audience = jwtSettings["Audience"] ?? throw new Exception("JWT audience not found");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiresInMinuntes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

		SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
