namespace GoBus.Models;

public class Route
{
    public int Id { get; set; }

    public string Departure { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public double Distance { get; set; }

    public int EstimatedDuration { get; set; }

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}