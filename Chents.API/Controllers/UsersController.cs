using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chents.Models.Models;

namespace Chents_GS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(AddHateoasLinks(users));
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(AddHateoasLinks(user));
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, AddHateoasLinks(user));
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(Guid id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(Guid id)
    {
        return _context.Users.Any(e => e.Id == id);
    }

    private dynamic AddHateoasLinks(User user)
    {
        return new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.CreatedAt,
            Links = new[]
            {
                new { Rel = "self", Href = Url.Action(nameof(GetUser), new { id = user.Id }) },
                new { Rel = "alerts", Href = Url.Action("GetUserAlerts", "Alerts", new { userId = user.Id }) }
            }
        };
    }

    private IEnumerable<dynamic> AddHateoasLinks(IEnumerable<User> users)
    {
        return users.Select(user => AddHateoasLinks(user));
    }
}