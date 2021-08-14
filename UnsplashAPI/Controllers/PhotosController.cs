using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnsplashAPI.Modules;
using UnsplashAPI.Services;

namespace UnsplashAPI.Controllers {

  [ApiController]
  [Route("[controller]")]
  [Authorize("User_Policy")]
  public class PhotosController: Controller {
    private readonly IPhotosServices _services;

    public PhotosController(IPhotosServices services) {
      _services = services;
    }

    [HttpGet("list")]
    public IActionResult GetAllPhotos(
      [FromQuery(Name = "s")] string label = "") 
    {
      var response = _services.GetAllPhotos(label);

      return (
        response.Status == 200
        ? Ok(response)
        : BadRequest(response)
      );
    }
    
    [HttpGet("item")]
    public IActionResult GetPhoto(
      [FromQuery(Name = "id")] int photoId) 
    {
      var response = _services.GetPhoto(photoId);

      return (
        response.Status == 200
        ? Ok(response)
        : BadRequest(response)
      );
    }

    [HttpDelete("remove")]
    public IActionResult DeletePhoto(
      [FromQuery(Name = "id")] string userId,
      [FromQuery(Name = "pid")] int photoId,
      [FromServices] IAuthorizationService _authorizationService)
    {
      var response = _services.DeletePhoto(
        userId, 
        photoId, 
        !_services.AuthorizeUser(_authorizationService, this.User, "Admin").Result);

      return (
        response.Status switch {
          200 => Ok(response),
          400 => BadRequest(response),
          401 => Unauthorized(response),
          _   => BadRequest()
        }
      );
    }

    [HttpPost("create")]
    public IActionResult CreatePhoto([FromBody] Photo photo)
    {
      var response = _services.CreatePhoto(photo);

      return (
        response.Status == 200
        ? CreatedAtAction(nameof(GetPhoto), new { photoId = photo.PhotoId }, response)
        : BadRequest(response)
      );
    }

    [HttpPut("update")]
    public IActionResult UpdatePhoto(
      [FromQuery(Name = "id")] string userId,
      [FromQuery] int photoId,
      [FromBody] Photo photo)
    {
      var response = _services.UpdatePhoto(userId, photoId, photo);

      return (
        response.Status == 200
        ? NoContent()
        : BadRequest(response)
      );
    }
  }
}