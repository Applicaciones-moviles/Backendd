namespace DriveSafe.Domain.Publishing.Models.Entities;

public class Vehicle : BaseModel
{
    public string Brand { get; set; }
    public string Model { get; set; }
    public int MaximumSpeed { get; set; }
    public int Consumption { get; set; }
    public string Dimensions { get; set; }
    public int Weight { get; set; }
    public int RentalCost { get; set; }
    
    public string Description { get; set; }
    
    public string Placa { get; set; }
    public string UrlImage { get; set; }
    public string RentStatus { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    public int OwnerId { get; set; }
}
