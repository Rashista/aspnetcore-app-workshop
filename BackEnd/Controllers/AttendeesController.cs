using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using ConferenceDTO;
using BackEnd.Infrastructure;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Attendees
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Attendee>>> GetAttendees()
        //{
        //    return await _context.Attendees.ToListAsync();
        //}

        // GET: api/Attendees/5
        [HttpGet("{username}")]
        public async Task<ActionResult<AttendeeResponse>> Get(string username)
        {
            var attendee = await _context.Attendees.Include(a => a.SessionAttendees)
                                                    .ThenInclude(sa => sa.Session)
                                                    .SingleOrDefaultAsync(a => a.UserName == username);

            if (attendee == null)
            {
                return NotFound();
            }

            var result = attendee.MapAttendeeResponse();

            return result;
        }

        //// PUT: api/Attendees/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutAttendee(int id, Attendee attendee)
        //{
        //    if (id != attendee.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(attendee).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AttendeeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Attendees
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AttendeeResponse>> Post(ConferenceDTO.Attendee input)
        {
            var existingAttendee = await _context.Attendees
                                    .Where(a => a.UserName == input.UserName)
                                    .FirstOrDefaultAsync();

            if (existingAttendee != null)
            {
                return Conflict(input);
            }

            var attendee = new Data.Attendee
            {
                FirstName = input.FirstName,
                Lastname = input.Lastname,
                EmailAddress = input.EmailAddress,
                UserName = input.UserName
            };

            _context.Attendees.Add(attendee);
            await _context.SaveChangesAsync();

            var result = attendee.MapAttendeeResponse();

            return CreatedAtAction(nameof(Get), new { username = result.UserName }, result);
        }

        [HttpPost("{username}/session/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AttendeeResponse>> AddSession(string username, int sessionId)
        {
            var attendee = await _context.Attendees.Include(a => a.SessionAttendees)
                                                        .ThenInclude(sa => sa.Session)
                                                        .SingleOrDefaultAsync(a => a.UserName == username);

            if (attendee == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(sessionId);

            if (session == null)
            {
                return BadRequest();
            }

            attendee.SessionAttendees.Add(new SessionAttendee
            {
                AttendeeId = attendee.Id,
                SessionId = session.Id
            });

            await _context.SaveChangesAsync();

            var result = attendee.MapAttendeeResponse();

            return result;
        }

        [HttpDelete("{username}/session/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> RemoveSession(string username, int sessionId)
        {
            var attendee = await _context.Attendees.Include(a => a.SessionAttendees)
                                                    .SingleOrDefaultAsync(a => a.UserName == username);

            if (attendee == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(sessionId);

            if (session == null)
            {
                return BadRequest();
            }

            var sessionAttendee = attendee.SessionAttendees.FirstOrDefault(sa => sa.SessionId == sessionId);
            attendee.SessionAttendees.Remove(sessionAttendee);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        //// DELETE: api/Attendees/5
        //[HttpDelete("{id}")]
        //public async Task<ActionResult<Attendee>> DeleteAttendee(int id)
        //{
        //    var attendee = await _context.Attendees.FindAsync(id);
        //    if (attendee == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Attendees.Remove(attendee);
        //    await _context.SaveChangesAsync();

        //    return attendee;
        //}

        //private bool AttendeeExists(int id)
        //{
        //    return _context.Attendees.Any(e => e.Id == id);
        //}
    }
}
