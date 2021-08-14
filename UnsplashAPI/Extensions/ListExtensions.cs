using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace UnsplashAPI.Extensions {
  public static class ListExtensions {
    public static IEnumerable<TResult> Map<TSourse, TResult>(
      this IEnumerable<TSourse> list, 
      Func<TSourse, TResult> func) 
      where TSourse: class 
      where TResult: class
    {
      List<TResult> resultsList = new List<TResult>();

      foreach(var item in list) {
        resultsList.Add(func(item));
      }

      return resultsList;
    }
  }
}