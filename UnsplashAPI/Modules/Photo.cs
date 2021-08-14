using System.ComponentModel.DataAnnotations;

namespace UnsplashAPI.Modules {
  public class Photo {

    [Key]
    public int PhotoId { get; set; }

    [MaxLength(100)]
    [Required]
    public string PhotoLabel { get; set; }

    [Url]
    [Required]
    public string PhotoUrl { get; set; }

    [Required]
    public string OwnerId { get; set; }
    public User Owner { get; set; }
  }
}