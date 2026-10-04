namespace GoBus.Models;

public class Seat
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public string SeatType { get; set; } = "Normal";

    public bool IsBooked { get; set; }

    public Trip? Trip { get; set; }
}