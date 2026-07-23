using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerProject.ApplicationDBContext;
using TaskTrackerProject.Entities;
using TaskTrackerProject.Models;

namespace TaskTrackerProject.ApiController
{
    [ApiController]
    [Authorize]
    [Route("api/ticket")]
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public TicketController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost("add")]
        public async Task<IActionResult> add([FromBody] TicketModel model)
        {
            try
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    Priority = model.Priority,
                    Status = model.Status,
                    StoryPoints = model.StoryPoints,
                    AssignedDate = model.AssignedDate,
                    DueDate = model.DueDate
                };

                // Save the ticket FIRST so it exists in the DB before any
                // TicketUser row tries to reference it via foreign key.
                await _appDbContext.AddAsync(ticket);
                await _appDbContext.SaveChangesAsync();

                if (model.ticketUser != null && model.ticketUser.Count > 0)
                {
                    foreach (var item in model.ticketUser)
                    {
                        await _appDbContext.AddAsync(new TicketUser
                        {
                            Id = Guid.NewGuid(),
                            TicketId = ticket.Id,
                            UserId = item.UserId
                        });
                    }

                    await _appDbContext.SaveChangesAsync();
                }

                return Ok(new { status = true, message = "Ticket added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("get")]
        public async Task<IActionResult> get()
        {
            try
            {
                var tickets = await _appDbContext.Tickets.ToListAsync();
                var ticketUsers = await _appDbContext.TicketUsers.ToListAsync();
                var userData = await _appDbContext.Signups.ToListAsync();

                // Manual join — returns ALL linked users per ticket, not just the first
                var data = tickets.Select(t =>
                {
                    var links = ticketUsers.Where(tu => tu.TicketId == t.Id).ToList();

                    var assignedUsers = links.Select(link =>
                    {
                        var user = userData.FirstOrDefault(u => u.Id == link.UserId);
                        return new
                        {
                            userId = link.UserId,
                            userName = user?.Name
                        };
                    }).ToList();

                    return new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Priority,
                        t.Status,
                        t.StoryPoints,
                        t.AssignedDate,
                        t.DueDate,
                        userIds = assignedUsers.Select(u => u.userId).ToList(),
                        ticketUser = assignedUsers
                    };
                });

                return Ok(new { status = true, Message = "Ticket Data", data = data, userData = userData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] TicketModel model)
        {
            try
            {
                var ticketData = await _appDbContext.Tickets
                    .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (ticketData == null)
                {
                    return NotFound(new
                    {
                        status = false,
                        message = "Ticket not found"
                    });
                }

                ticketData.Title = model.Title;
                ticketData.Description = model.Description;
                ticketData.Priority = model.Priority;
                ticketData.Status = model.Status;
                ticketData.StoryPoints = model.StoryPoints;
                ticketData.AssignedDate = model.AssignedDate;
                ticketData.DueDate = model.DueDate;

                _appDbContext.Tickets.Update(ticketData);

                await _appDbContext.SaveChangesAsync();

                var existingLinks = await _appDbContext.TicketUsers
                    .Where(tu => tu.TicketId == model.Id)
                    .ToListAsync();

                if (existingLinks.Count > 0)
                {
                    _appDbContext.TicketUsers.RemoveRange(existingLinks);
                    await _appDbContext.SaveChangesAsync();
                }

                if (model.ticketUser != null && model.ticketUser.Count > 0)
                {
                    foreach (var item in model.ticketUser)
                    {
                        await _appDbContext.TicketUsers.AddAsync(new TicketUser
                        {
                            Id = Guid.NewGuid(),
                            TicketId = model.Id,
                            UserId = item.UserId
                        });
                    }

                    await _appDbContext.SaveChangesAsync();
                }

                return Ok(new
                {
                    status = true,
                    message = "Ticket updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("updateStatus")]
        public async Task<IActionResult> updateStatus([FromBody] TicketModel model)
        {
            try
            {
                var ticketData = await _appDbContext.Tickets
                    .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (ticketData == null)
                {
                    return NotFound(new { status = false, message = "Ticket not found" });
                }

                ticketData.Status = model.Status;
                _appDbContext.Tickets.Update(ticketData);
                await _appDbContext.SaveChangesAsync();

                return Ok(new { status = true, message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPost("updatePriority")]
        public async Task<IActionResult> updatePriority([FromBody] TicketModel model)
        {
            try
            {
                var ticketData = await _appDbContext.Tickets
                    .FirstOrDefaultAsync(t => t.Id == model.Id);

                if (ticketData == null)
                {
                    return NotFound(new { status = false, message = "Ticket not found" });
                }

                ticketData.Priority = model.Priority;
                _appDbContext.Tickets.Update(ticketData);
                await _appDbContext.SaveChangesAsync();

                return Ok(new { status = true, message = "Priority updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("delete")]
        public async Task<IActionResult> delete([FromQuery] Guid id)
        {
            try
            {
                var deleteData = await _appDbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id);
                if (deleteData != null)
                {
                    var links = await _appDbContext.TicketUsers
                        .Where(tu => tu.TicketId == id)
                        .ToListAsync();

                    if (links.Count > 0)
                    {
                        _appDbContext.TicketUsers.RemoveRange(links);
                        await _appDbContext.SaveChangesAsync();
                    }

                    _appDbContext.Tickets.Remove(deleteData);
                    await _appDbContext.SaveChangesAsync();
                    return Ok(new { status = true, message = "Ticket deleted successfully" });
                }
                else
                {
                    return NotFound(new { status = false, message = "Ticket not found" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("getUser")]
        public async Task<IActionResult> getUser()
        {
            try
            {
                var tickets = await _appDbContext.Signups.ToListAsync();
                return Ok(new { status = true, Message = "Ticket Data", data = tickets });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }
    }
}