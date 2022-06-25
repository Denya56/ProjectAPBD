using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Project.Context;
using Project.DTOs.UserAuth;
using Project.Exceptions;
using Project.Helper;
using Project.Models.UserAuth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project.Services.UserAuth
{
    [AllowAnonymous]
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly ProjectContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationServices(ProjectContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task RegisterAsync(UserDTO userDTO)
        {
            var hasPassSalt = SecurityHelper.GetHashPasswordSalt(userDTO.Password);

            var AppUser = new User()
            {
                Login = userDTO.Login,
                Password = hasPassSalt.Item1,
                Salt = hasPassSalt.Item2,
                RefreshToken = SecurityHelper.GetRefreshToken(),
                RefreshTokenExpiration = DateTime.Now.AddDays(1)
            };
            await _context.Users.AddAsync(AppUser);
            await _context.SaveChangesAsync();
        }
        public async Task<AccountResponseDTO> LoginAsync(UserDTO loginRequest)
        {
            User? appUser = _context.Users.FirstOrDefault(x => x.Login == loginRequest.Login);
            if (appUser == null)
            {
                throw new NotFoundException("User does not exist");
            }

            string passwordHash = appUser.Password;
            string currentHashPass = SecurityHelper.GetPasswordWithSalt(loginRequest.Password, appUser.Salt);

            if (passwordHash != currentHashPass)
            {
                throw new UnauthorizedException("Bad password");
            }
            else
            {
                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.Name, appUser.IdUser.ToString()),
                    new Claim(ClaimTypes.Role,"admin"),
                    new Claim(ClaimTypes.Role,"user")
                };
                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));

                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: _configuration["Issuer"],
                    audience: _configuration["Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(2),
                    signingCredentials: creds
                    );

                /*appUser.RefreshToken = SecurityHelper.GetRefreshToken();
                appUser.RefreshToken = Guid.NewGuid().ToString();
                appUser.RefreshTokenExpiration = DateTime.Now.AddDays(1);*/
                await _context.SaveChangesAsync();

                var result = new AccountResponseDTO
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = appUser.RefreshToken,
                };
                return result;
            }
        }
        public async Task<AccountResponseDTO> RefreshAsync(string refreshToken)
        {
            User? appUser = _context.Users.FirstOrDefault(x => x.RefreshToken == refreshToken);
            if (appUser == null || appUser.RefreshTokenExpiration < DateTime.Now)
            {
                throw new BadHttpRequestException("Refresh token expired");
            }
            //var login = SecurityHelper.GetUserIdFromAccToken(token.Replace("Bearer ", ""), _configuration["Secret"]);

            Claim[] claims = new[]
            {
                    new Claim(ClaimTypes.Name,"name"),
                    new Claim(ClaimTypes.Role,"admin"),
                    new Claim(ClaimTypes.Role,"user")
                };
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken newTok = new JwtSecurityToken(
                issuer: _configuration["Issuer"],
                audience: _configuration["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(3),
                signingCredentials: creds
                );

            /*appUser.RefreshToken = SecurityHelper.GetRefreshToken();
            appUser.RefreshTokenExpiration = DateTime.Now.AddDays(1);*/
            await _context.SaveChangesAsync();

            var result = new AccountResponseDTO
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newTok),
                RefreshToken = appUser.RefreshToken
            };
            return result;
        }
    }
}
