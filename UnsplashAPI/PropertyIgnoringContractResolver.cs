using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Linq;
using UnsplashAPI.Modules;
using Microsoft.AspNetCore.Identity;

namespace UnsplashAPI {
  public class PropertyIgnoringContractResolver : DefaultContractResolver {
    private static User user = new();
    private readonly Dictionary<string, string[]> _ignoredPropertiesContainer = new Dictionary<string, string[]>
    {
      { 
        typeof(IdentityUser).ToString() + "`1[System.String]", 
        new string[] { 
          nameof(user.PasswordHash), 
          nameof(user.NormalizedUserName), 
          nameof(user.NormalizedEmail), 
          nameof(user.ConcurrencyStamp), 
          nameof(user.PhoneNumberConfirmed), 
          nameof(user.TwoFactorEnabled), 
          nameof(user.TwoFactorEnabled), 
          nameof(user.SecurityStamp)} 
      },
      { 
        typeof(User).ToString(),
        new string[] { 
          nameof(user.UserPhotos)
        }
      }
    };

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
      JsonProperty property = base.CreateProperty(member, memberSerialization);
      string[] ignoredPropertiesOfType;
      if (this._ignoredPropertiesContainer.TryGetValue(member.DeclaringType.ToString(), out ignoredPropertiesOfType)) {
        if (ignoredPropertiesOfType.Contains(member.Name)) {
          property.ShouldSerialize = instance => false;
          return property;
        }
      }

      return property;
    }
  }
}
