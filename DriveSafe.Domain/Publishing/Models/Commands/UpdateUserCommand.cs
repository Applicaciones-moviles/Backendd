using System.ComponentModel.DataAnnotations;

namespace DriveSafe.Domain.Publishing.Models.Commands;

public class UpdateUserCommand
{
    [Required] public string FullName { get; set; }
    
    [Required] public string DNI { get; set; }
    
    [Required] public int Cellphone { get; set; }
    
    [Required] public string Gmail { get; set; }
    
    [Required] public string Password { get; set; }
    
    [Required] public string Type { get; set; }
}