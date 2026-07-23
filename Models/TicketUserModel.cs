using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskTrackerProject.Models
{
    public class TicketUserModel
    {
        public Guid Id { get; set; }

        // Foreign Key to Ticket
        public Guid TicketId { get; set; }

        // Foreign Key to Signup
        public Guid UserId { get; set; }
    
    }
}