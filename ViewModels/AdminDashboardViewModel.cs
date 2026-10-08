namespace GoBus.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalCustomers { get; set; }

    public int TotalUsers { get; set; }

    public int TotalBusCompanies { get; set; }

    public int TotalBuses { get; set; }

    public int TotalRoutes { get; set; }

    public int TotalTrips { get; set; }

    public int TodayTrips { get; set; }

    public int TotalBookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int CancelledBookings { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal ConfirmedRevenue { get; set; }
}