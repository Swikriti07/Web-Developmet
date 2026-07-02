using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerProject.ApplicationDBContext;
using TaskTrackerProject.Entities;

namespace TaskTrackerProject.ApiController
{
    [ApiController]
    [Route("api/signup")]
    public class SignUpController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        public SignUpController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
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
    }


}