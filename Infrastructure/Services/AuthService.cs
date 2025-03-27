using Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<object> AuthenticateWithGoogleAsync(string token)
        {
            try
            {
              
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);

              
                var jwtToken = GenerateJwtToken(payload);

                return new
                {
                    UserId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified,
                    Token = jwtToken
                };
            }
            catch (Exception ex)
            {
                return new { Error = $"Google authentication failed: {ex.Message}" };
            }
        }

        private string GenerateJwtToken(GoogleJsonWebSignature.Payload payload)
        {
            var secretKey = _config["JwtSettings:SecretKey"];
            var issuer = _config["JwtSettings:Issuer"];
            var audience = _config["JwtSettings:Audience"];
            var expirationMinutesString = _config["JwtSettings:ExpirationInMinutes"];


            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(expirationMinutesString))
            {
                throw new ArgumentNullException("JWT Secret Key hoặc Expiration không được để trống!");
            }

            if (!int.TryParse(expirationMinutesString, out int expirationMinutes))
            {
                throw new FormatException("ExpirationInMinutes phải là số nguyên hợp lệ!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub, payload.Subject),
                new Claim(JwtRegisteredClaimNames.Email, payload.Email),
                new Claim(JwtRegisteredClaimNames.Name, payload.Name),
                new Claim("picture", payload.Picture ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
