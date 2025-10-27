using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[Authorize] //Access only with valid JWT
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly List<User> _users;

    public UsersController(List<User> users)
    {
        _users = users;
    }

    // GET: Test exeption handling middleware
    [HttpGet("error")]
    public IActionResult GetError()
    {
        throw new Exception("Test exception");
    }


    // GET: All Users
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        if (_users == null || !_users.Any())
            return NotFound("No users stored in database");

        return Ok(_users);
    }

    // GET: Single User
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> GetUserById(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound($"No user found with id {id}.");

        return Ok(user);
    }

    // POST: Add new User
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<User> AddUser([FromBody] User newUser)
    {
        if (string.IsNullOrWhiteSpace(newUser.FirstName) ||
            string.IsNullOrWhiteSpace(newUser.LastName) ||
            string.IsNullOrWhiteSpace(newUser.Email) ||
            string.IsNullOrWhiteSpace(newUser.Address) ||
            string.IsNullOrWhiteSpace(newUser.Job) ||
            newUser.Age <= 0)
        {
            return BadRequest("All fields must filled with valid data.");
        }
        if (!newUser.Email.Contains("@") || !newUser.Email.Contains("."))
            return BadRequest("Email address not valid.");
        
        if (newUser.Age < 1 || newUser.Age > 100)
            return BadRequest("User age must be between 1 and 100");

        newUser.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
        _users.Add(newUser);
        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }

    // PUT: Actualize job data 
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateUserDetails(int id, [FromBody] string updatedJob)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound($"No User found with ID {id}.");

        if (string.IsNullOrWhiteSpace(updatedJob))
            return BadRequest("Job must not be empty.");

        user.Job = updatedJob;
        return NoContent();
    }

    // DELETE: Delete User
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound($"No User found with ID {id}.");

        _users.Remove(user);
        return Ok(user);
    }
}
