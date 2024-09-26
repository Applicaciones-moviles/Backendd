using System.ComponentModel.DataAnnotations;

namespace DriveSafe.Domain.Publishing.Models.Commands;

public class CreateVehicleCommand
{
    [Required] public string Brand { get; set; }
    [Required] public string Model { get; set; }
    [Required] public int MaximumSpeed { get; set; }
    [Required] public int Consumption { get; set; }
    [Required] public string Dimensions { get; set; }
    [Required] public int Weight { get; set; }
    [Required] public string Placa { get; set; }
    [Required] public string Description { get; set; }
    [Required] public int RentalCost { get; set; }
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required] public string UrlImage { get; set; }
    [Required] public string RentStatus { get; set; }
    [Required] public int OwnerId { get; set; }
}