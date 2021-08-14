using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UnsplashAPI.Data;
using UnsplashAPI.Modules;
using UnsplashAPI.Extensions;

namespace UnsplashAPI.Services {

  public interface IUsersServices {
    Response Login(string username, string password);
    Task<Response> Register(string username, string password, bool isAdmin);
    Task Logout();
    Response CheckPassword(string username, string password);
  }

  public class UsersServices : IUsersServices {

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public UsersServices(
      UserManager<User> userManager,
      SignInManager<User> signInManager,
      RoleManager<IdentityRole> roleManager,
      AppDbContext context)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _context = context;
      _roleManager = roleManager;
    }

    /* Private Functions */
    private void CheckAndFindUser(
      ref Response response, 
      string username, 
      string password,
      Func<User, string, bool, bool, Task<SignInResult>> loginFunc = null,
      Func<User, string, bool, Task<SignInResult>> checkFunc = null) 
    {
      if(username is null || password is null) {
        response.StatusCode400(
          new ErrorMessage("EmptyRequest", "Please enter the username and the password")
        );
      }

      var user = _context.Users.FirstOrDefault(u => u.UserName == username);

      if(user is null) {
        response.StatusCode400(
          new ErrorMessage("WrongUsername", "The username is wrong")
        );
      } else {
        var results = loginFunc?.Invoke(user, password, false, false).Result;
        results ??= checkFunc?.Invoke(user, password, false).Result;

        if(results.Succeeded) {
          response.LoadData(true, 200, user);
        } else {
          response.StatusCode400(
            new ErrorMessage("WrongPassword", "The password is wrong")
          );
        }
      }
    }

    private async Task AddUserToRole(User user, [NotNull] string roleName = "") {
      if(!_roleManager.RoleExistsAsync(roleName).Result) {
        IdentityRole role = new() { Name = roleName };

        var createRoleResults = await _roleManager.CreateAsync(role);

        if(createRoleResults.Succeeded) {
          var claimResults = await _userManager.AddToRoleAsync(user, roleName);
        }
      } else {
        var claimResults = await _userManager.AddToRoleAsync(user, roleName);
      }
    }

    /* Public Functions */
    public Response Login(string username, string password) {
      Response response = new();

      CheckAndFindUser(ref response, username, password, _signInManager.PasswordSignInAsync);

      return response;
    }
    
    public Response CheckPassword(string username, string password) {
      Response response = new();

      CheckAndFindUser(ref response, username, password, checkFunc: _signInManager.CheckPasswordSignInAsync);

      return response;
    }

    public async Task<Response> Register(
      string username, 
      string password,
      bool isAdmin)
    {
      Response response = new();

      if(username is null || password is null) {
        response.StatusCode400(
          new ErrorMessage("EmptyRequest", "Please enter the username and the password")
        );
      } else {
        User user = new() { UserName = username };

        var createResults = await _userManager.CreateAsync(user, password);

        AddUserToRole(user, "User").Wait();

        if(isAdmin) {
          AddUserToRole(user, "Admin").Wait();
        } 
        
        if(createResults.Succeeded) {
          response.LoadData(true, 201, new { Message = "Register successed"});
        } else {
          var errors = createResults
            .Errors
            .Map<IdentityError, ErrorMessage>(e => new ErrorMessage(e.Code, e.Description))
            .ToList();

          response.StatusCode400(errors);
        }
      }

      return response;
    }

    public async Task Logout() 
      => await _signInManager.SignOutAsync();
  }
}