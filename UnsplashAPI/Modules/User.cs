using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace UnsplashAPI.Modules {
  public class User: IdentityUser {

    [JsonIgnore]
    public List<Photo> UserPhotos { get; set; }
  }
}