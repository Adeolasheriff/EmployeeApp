﻿using EmployeeAdminPortal.Model.Entity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeAdminPortal
{
    public class Jwt
    {
        private readonly IConfiguration _configuration;

        public Jwt(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public string GenerateJwtToken(User user)
        {

            var claims = new[]
            {
                   new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                   new Claim("Name",user.Name)

           };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
         issuer: _configuration["Jwt:Issuer"],
         audience: null,
         claims: claims,
         expires: DateTime.Now.AddMinutes(30),
          signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }



    }
}
