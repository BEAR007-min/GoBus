namespace GoBus.Models;

public class Booking
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int TripId { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Confirmed";

    public Customer? Customer { get; set; }

    public Trip? Trip { get; set; }

    public ICollection<BookingDetail> BookingDetails { get; set; }
        = new List<BookingDetail>();
}