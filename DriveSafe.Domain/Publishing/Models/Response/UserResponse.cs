namespace DriveSafe.Domain.Publishing.Models.Response;

public class UserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string DNI { get; set; }
    public int Cellphone { get; set; }
    public string Gmail { get; set; }
    public string Password { get; set; }
    public string Type { get; set; }
}