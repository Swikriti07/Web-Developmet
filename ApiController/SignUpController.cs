using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskTrackerProject.ApplicationDBContext;
using TaskTrackerProject.Entities;

namespace TaskTrackerProject.ApiController
{
    [ApiController]
    [Route("api/signup")]
    public class SignUpController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _config;
        public SignUpController(AppDbContext appDbContext, IConfiguration config)
        {
            _appDbContext = appDbContext;
            _config = config;
        }

        [HttpPost("add")]
        public async Task<IActionResult> add([FromBody] Signup model)
        {
            try
            {
                _appDbContext.Signups.Add(model);
                await _appDbContext.SaveChangesAsync();
                return Ok(new{Status=true, Message="User added successfully"});
            }
            catch (Exception)
            {
                return Ok(new{Status=false, Message="Failed to add user"});
            }
        }

        [HttpGet("get")]
        public async Task<IActionResult> get()
        {
            try
            {
                var signupData = await _appDbContext.Signups.ToListAsync();
                return Ok(new{Status=true, Message="User added successfully", Data=signupData});
            }
            catch (Exception)
            {
                return Ok(new{Status=false, Message="Failed to retrieve users"});
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> delete(Guid id)
        {
            try
            {
                var signupData = await _appDbContext.Signups.FindAsync(id);
                if (signupData == null)
                {
                    return NotFound(new { Status = false, Message = "User not found" });
                }

                _appDbContext.Signups.Remove(signupData);
                await _appDbContext.SaveChangesAsync();
                return Ok(new { Status = true, Message = "User deleted successfully" });
            }
            catch (Exception)
            {
                return Ok(new { Status = false, Message = "Failed to delete user" });
            }
        }
        
        [HttpPut("update/{id}")]
        public async Task<IActionResult> update(Guid id, [FromBody] Signup model)
        {
            try
            {
                var signupData = await _appDbContext.Signups.FindAsync(id);
                if (signupData == null)
                {
                    return NotFound(new { Status = false, Message = "User not found" });
                }

                signupData.Name = model.Name;
                signupData.Address = model.Address;
                signupData.Email = model.Email;
                signupData.Password = model.Password;
                signupData.DOB = model.DOB;

                await _appDbContext.SaveChangesAsync();
                return Ok(new { Status = true, Message = "User updated successfully" });
            }
            catch (Exception)
            {
                return Ok(new { Status = false, Message = "Failed to update user" });
            }
        }
    
       [HttpPost("login")]
       public async Task<IActionResult> login([FromBody] Signup model)
        {
            try
            {
             var userExist= await _appDbContext.Signups.AnyAsync(x=>x.Email==model.Email && x.Password==model.Password);
                if (userExist)
                {
                    return Ok(new{Status=true, Message="Login successful", Token=GenerateToken(model.Email, "default")});
                }
                else
                {
                    return Ok(new{Status=false,Message="Invalid email or password"});
                }  
            }
            catch(Exception ex)
            {
               return Ok(new{Status=false, Message="Login failed", Error=ex.Message}); 
            }
        }
    
    public string GenerateToken(string username, string role)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Define claims mapping user identity
        var claims = new Dictionary<string, object>
        {
            { ClaimTypes.Name, username },
            { ClaimTypes.Role, role },
            { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() }
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            Claims = claims,
            Expires = DateTime.UtcNow.AddMinutes(15), // Set brief token lifespans
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor); // Generates raw string JWT token
    }
}
    }

