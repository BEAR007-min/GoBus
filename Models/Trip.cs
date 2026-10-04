namespace GoBus.Models;

public class Trip
{
    public int Id { get; set; }

    public int BusCompanyId { get; set; }

    public int RouteId { get; set; }

    public int BusId { get; set; }

    public DateTime DepartureTime { get; set; }

    public DateTime ArrivalTime { get; set; }

    public decimal Price { get; set; }

    public int AvailableSeats { get; set; }


    // ==========================================
    // QUAN HỆ
    // ==========================================

    public BusCompany? BusCompany { get; set; }

    public Route? Route { get; set; }

    public Bus? Bus { get; set; }


    // ==========================================
    // DANH SÁCH GHẾ
    // ==========================================

    public ICollection<Seat> Seats { get; set; }
        = new List<Seat>();
}