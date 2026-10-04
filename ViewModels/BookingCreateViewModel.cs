namespace GoBus.ViewModels;

public class BookingCreateViewModel
{
    public int TripId { get; set; }

    public List<int> SeatIds { get; set; } = new();
}