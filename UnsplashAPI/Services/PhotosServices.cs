using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UnsplashAPI.Data;
using UnsplashAPI.Modules;

namespace UnsplashAPI.Services {
  public interface IPhotosServices {
    Response GetAllPhotos(string label);
    Response GetPhoto(int photoId);
    Response DeletePhoto(string userId, int photoId, bool shouldCheck);
    Response CreatePhoto(Photo photo);
    Response UpdatePhoto(string userId, int photoId, Photo photo);
    Task<bool> AuthorizeUser(IAuthorizationService authorizationService, ClaimsPrincipal user, params string[] roles);
  }

  public class PhotosServices: IPhotosServices {
    private readonly AppDbContext _context;

    public PhotosServices(AppDbContext context) {
      _context = context;
    }
  
    /* Private Functions */
    private void FindPhoto(
      ref Response response,
      int photoId,
      Action<Photo> action) 
    {
      var photo = _context.Photos.Find(photoId);
      
      if(photo is not null) {
        action(photo);
      } else {
        response.StatusCode400(
          new ErrorMessage("ItemNotFound", "There is no such photo with this ID")
        );
      }
    }

    private bool CheckOwner(string userId, int photoId) 
      => (_context.Photos.Find(photoId).OwnerId == userId);

    /* Public Functions */
    public Response GetAllPhotos(string label = "") {
      Response response = new();

      var photos = _context.Photos.Where(p => p.PhotoLabel.Contains(label)).ToList();
      photos.ForEach(p => {
        p.Owner = _context.Users.Find(p.OwnerId);
      });

      response.StatusCode200(photos);

      return response;
    }

    public Response GetPhoto(int photoId) {
      Response response = new();

      FindPhoto(ref response, photoId, photo => {
        photo.Owner = _context.Users.Find(photo.OwnerId);
        response.StatusCode200(photo);
      });

      return response;
    }

    public Response DeletePhoto(
      string userId,
      int photoId,
      bool shouldCheck) 
    {
      Response response = new();

      FindPhoto(ref response, photoId, photo => {
        if(!shouldCheck || CheckOwner(userId, photoId)) {
          photo.Owner = _context.Users.Find(photo.OwnerId);
          response.StatusCode200(photo);
          _context.Photos.Remove(photo);
          _context.SaveChanges();
        } else {
          response.StatusCode401(
            new ErrorMessage("UnAuthorized", "You don't own this photo")
          );
        }
      });

      return response;
    }

    public Response CreatePhoto(Photo photo) {
      Response response = new();

      if(photo is not null) {
        var owner = _context.Users.Find(photo.OwnerId);

        if(owner is not null) {
          _context.Photos.Add(photo);

          response.StatusCode200(photo);
          
          _context.SaveChanges();
        }
        else {
          response.StatusCode400(
            new ErrorMessage("UserNotFound", "Make sure to enter the right owner id")
          );
        }
      } else {
        response.StatusCode400(
          new ErrorMessage("InvalidData", "Make sure to enter all the data required")
        );
      }

      return response;
    }

    public Response UpdatePhoto(string userId, int photoId, Photo photo) {
      Response response = new();

      if(CheckOwner(userId, photoId)) {
        FindPhoto(ref response, photoId, p => {
          p.PhotoLabel = photo.PhotoLabel;
          p.PhotoUrl = photo.PhotoUrl;
          var results = _context.SaveChanges();
          if(results == 1) {
            response.StatusCode200(null);
          } else {
            response.StatusCode400(
              new ErrorMessage("InvalidData", "Make sure to enter all the required data in the right type")
            );
          }
        });
      }

      return response;
    }

    public async Task<bool> AuthorizeUser(
      IAuthorizationService authorizationService, 
      ClaimsPrincipal user, 
      params string[] roles) 
    {
      var builder = new AuthorizationPolicyBuilder();
      var policy = builder
        .RequireRole(roles)
        .Build();

      var authResults = await authorizationService.AuthorizeAsync(user, policy);

      return authResults.Succeeded;
    }
  }
}