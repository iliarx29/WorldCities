using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorldCitiesAPI.Data.Models;

[Table("Cities")]
[Index(nameof(Name))]
[Index(nameof(Lat))]
[Index(nameof(Lon))]
public class City
{
    [Key]
    [Required]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lon { get; set; }
    public int CountryId { get; set; }
    public Country? Country { get; set; }
}
