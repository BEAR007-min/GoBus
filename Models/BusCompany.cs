namespace GoBus.Models;

public class BusCompany
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<Bus> Buses { get; set; } = new List<Bus>();

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}