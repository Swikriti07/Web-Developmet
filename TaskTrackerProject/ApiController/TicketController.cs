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
    [Route("api/ticket")]
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
    public TicketController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    [HttpPost("add")]
    public async Task<IActionResult> add([FromBody] Ticket model)
        {
            try
            {
              await  _appDbContext.AddAsync(model);
              await _appDbContext.SaveChangesAsync();
              return Ok(new { status = true, message = "Ticket added successfully" });
            }
            catch(Exception ex)
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
            return Ok(new { status = true, Message="Ticket Data",data = tickets });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, message = ex.Message });
        }
    }
    
    [HttpPost("update")]
public async Task<IActionResult> Update([FromBody] Ticket model)
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
        ticketData.DueDate = model.DueDate;

        _appDbContext.Tickets.Update(ticketData);
        await _appDbContext.SaveChangesAsync();

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

[HttpGet("delete")]
public async Task<IActionResult> delete([FromQuery] Guid id)
        {
            try
            {
                var deleteData=await _appDbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id);
                if (deleteData != null)
                {
                    _appDbContext.Tickets.Remove(deleteData);
                    await _appDbContext.SaveChangesAsync();
                    return Ok(new { status = true, message = "Ticket deleted successfully" });
                }
                else
                {
                    return NotFound(new { status = false, message = "Ticket not found" });
                }
            }catch(Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }
    }    
}
