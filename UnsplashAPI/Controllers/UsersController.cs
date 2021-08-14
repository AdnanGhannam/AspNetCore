using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UnsplashAPI.Services;

namespace UnsplashAPI.Controllers {

  [ApiController]
  [Route("[controller]")]
  public class UsersController: Controller {
    
    private readonly IUsersServices _services;

    public UsersController(IUsersServices services)
      => _services = services;

    // Login 
    [HttpPost("login")]
    public IActionResult Login(
      [FromHeader] string username,
      [FromHeader] string password)
    {
      var response = _services.Login(username, password);

      return (
        response.Status == 200
        ? Ok(response)
        : BadRequest(response)
      );
    }

    // Register
    [HttpPost("register")]
    public IActionResult Register(
      [FromHeader] string username,
      [FromHeader] string password,
      [FromQuery(Name = "p")] string adminPassword,
      [FromServices] IConfiguration _config)
    {
      var isAdmin = (_config["Passwords:Admin"] == adminPassword);
      var response = _services.Register(username, password, isAdmin).Result;

      return (
        response.Status == 201
        ? Ok(response)
        : BadRequest(response)
      );
    }

    // Logout
    [HttpPost("logout")]
    public IActionResult Logout()
      => Ok(_services.Logout());

    [HttpPost("check")]
    public IActionResult CheckPassword(
      [FromHeader] string username, 
      [FromHeader] string password)
      => Ok(_services.CheckPassword(username, password));
  }
}