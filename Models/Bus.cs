namespace GoBus.Models;

public class Bus
{
    public int Id { get; set; }

    public int BusCompanyId { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public string BusType { get; set; } = string.Empty;

    public int TotalSeats { get; set; }

    public BusCompany? BusCompany { get; set; }

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}