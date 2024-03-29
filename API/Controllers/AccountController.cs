using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserModel>> Register(RegisterModel registerModel){

            if(await UserExists(registerModel.Username)) return BadRequest("Username is taken");

            using var hmac= new HMACSHA512();

            var user = new AppUser
            {
                UserName=registerModel.Username.ToLower(),
                PasswordHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(registerModel.Password)),
                PasswordSalt= hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserModel
            {
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserModel>> Login (LoginModel loginModel)
        {
            var user= await _context.Users.SingleOrDefaultAsync(user => user.UserName==loginModel.Username);

            if(user==null) return Unauthorized("Invalid username");

            using var hmac= new HMACSHA512(user.PasswordSalt);

            var computedHash= hmac.ComputeHash(Encoding.UTF8.GetBytes(loginModel.Password));

            for(int i=0;i<computedHash.Length;i++){
                if(computedHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

             return new UserModel
            {
                Username=user.UserName,
                Token=_tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username){
            return await _context.Users.AnyAsync(user =>user.UserName==username.ToLower());
        }
    }
}