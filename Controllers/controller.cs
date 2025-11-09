using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

public class User
{
    public string? userName { get; set; }

    [Required]
    public required int userId { get; set; }

    public string? userDetails { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TechHiveController : ControllerBase
{
    static List<User> users = new List<User>
    {
        new User {userName = "Minseok Kim", userId = 1, userDetails = "This is Minseok"},
        new User {userName = "Yuta Bordes", userId = 2, userDetails = "This is Yuta"},
        new User {userName = "Ian", userId = 3, userDetails = "This is Ian"}
    };

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUserList()
    {
        return Ok(users);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUser(int id)
    {
        try
        {
            var userWithId = users.SingleOrDefault(user => user.userId == id);
            if (userWithId == null)
            {
                return NotFound($"User with id {id} does not exist.");
            }
            return Ok(userWithId);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AddUser(User user)
    {
        try
        {
            var currentUser = users.SingleOrDefault(currentUser => currentUser.userId == user.userId);
            if (currentUser != null) // user already has the same userid then update the user id
            {
                int maxId = users.Max(u => u.userId);
                user.userId = maxId + 1;
            }
            users.Add(user);
            return Ok(users);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateUser(int id, User userToUpdate)
    {
        if (userToUpdate == null) return BadRequest();

        var currentUserToUpdate = users.SingleOrDefault(user => user.userId == id);
        if (currentUserToUpdate == null) return BadRequest();
        currentUserToUpdate.userDetails = userToUpdate.userDetails;

        return Ok(currentUserToUpdate);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DeleteUser(int id)
    {
        var userToDelete = users.SingleOrDefault(user => user.userId == id);
        if (userToDelete == null) return BadRequest();
        users.Remove(userToDelete);
        return Ok(users);
    }

}

