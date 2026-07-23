using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTrackerProject.Entities

{
    public class Signup
    {
     public  Guid Id { get; set; }  
     public string? Name { get; set; }
     public string? Address { get; set; } 
     public required string Email { get; set; } 
     public required string Password { get; set; } 
     public DateTime DOB { get; set; }

    }
}