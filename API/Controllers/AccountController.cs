using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext Context;
        private readonly ITokenService TokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            Context = context;
            TokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            using var hmac = new HMACSHA512();

            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken!");

            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            Context.AppUsers.Add(user);
            await Context.SaveChangesAsync();

            return new UserDto()
            {
                Username = user.UserName,
                Token = TokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // SingleOrDefaultAsync: throw an exception if there's a duplicate result.
            var user = await Context.AppUsers.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username");

            var hmac = new HMACSHA512(user.PasswordSalt);

            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");

            }
            return new UserDto()
            {
                Username = user.UserName,
                Token = TokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await Context.AppUsers.AnyAsync(x => x.UserName == username.ToLower());
        }

    }
}