using GoBus.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminStatisticsController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminStatisticsController(GoBusDbContext context)
    {
        _context = context;
    }

    // GET: /AdminStatistics
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // =========================
        // THỐNG KÊ TỔNG QUAN
        // =========================

        var totalCustomers =
            await _context.Customers.CountAsync();

        var totalUsers =
            await _context.Users.CountAsync();

        var totalBusCompanies =
            await _context.BusCompanies.CountAsync();

        var totalBuses =
            await _context.Buses.CountAsync();

        var totalRoutes =
            await _context.Routes.CountAsync();

        var totalTrips =
            await _context.Trips.CountAsync();

        var totalBookings =
            await _context.Bookings.CountAsync();

        var confirmedBookings =
            await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed");

        var cancelledBookings =
            await _context.Bookings
                .CountAsync(b => b.Status == "Cancelled");


        // =========================
        // DOANH THU
        // =========================

        var totalRevenue =
            await _context.Bookings
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

        var confirmedRevenue =
            await _context.Bookings
                .Where(b => b.Status == "Confirmed")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

        var cancelledRevenue =
            await _context.Bookings
                .Where(b => b.Status == "Cancelled")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;


        // =========================
        // VÉ HÔM NAY
        // =========================

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var todayBookings =
            await _context.Bookings
                .CountAsync(b =>
                    b.BookingDate >= today &&
                    b.BookingDate < tomorrow);

        var todayRevenue =
            await _context.Bookings
                .Where(b =>
                    b.BookingDate >= today &&
                    b.BookingDate < tomorrow &&
                    b.Status == "Confirmed")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;


        // =========================
        // CHUYẾN XE HÔM NAY
        // =========================

        var todayTrips =
            await _context.Trips
                .CountAsync(t =>
                    t.DepartureTime >= today &&
                    t.DepartureTime < tomorrow);


        // =========================
        // GHẾ / GIƯỜNG
        // =========================

        var totalSeats =
            await _context.Seats.CountAsync();

        var bookedSeats =
            await _context.Seats
                .CountAsync(s => s.IsBooked);

        var availableSeats =
            totalSeats - bookedSeats;


        // =========================
        // TOP NHÀ XE
        // =========================

        var topBusCompanies =
            await _context.Bookings
                .Where(b =>
                    b.Status == "Confirmed" &&
                    b.Trip != null &&
                    b.Trip.BusCompany != null)
                .GroupBy(b => new
                {
                    b.Trip!.BusCompanyId,
                    CompanyName = b.Trip.BusCompany!.Name
                })
                .Select(g => new
                {
                    Name = g.Key.CompanyName,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();


        // =========================
        // TOP TUYẾN XE
        // =========================

        var topRoutes =
            await _context.Bookings
                .Where(b =>
                    b.Status == "Confirmed" &&
                    b.Trip != null &&
                    b.Trip.Route != null)
                .GroupBy(b => new
                {
                    b.Trip!.RouteId,
                    Departure = b.Trip.Route!.Departure,
                    Destination = b.Trip.Route.Destination
                })
                .Select(g => new
                {
                    Route =
                        g.Key.Departure +
                        " → " +
                        g.Key.Destination,

                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToListAsync();


        // =========================
        // DỮ LIỆU 7 NGÀY GẦN NHẤT
        // =========================

        var startDate =
            today.AddDays(-6);

        var recentBookings =
            await _context.Bookings
                .Where(b =>
                    b.BookingDate >= startDate &&
                    b.BookingDate < tomorrow)
                .ToListAsync();

        var last7Days = new List<object>();

        for (var date = startDate.Date;
             date <= today.Date;
             date = date.AddDays(1))
        {
            var nextDate = date.AddDays(1);

            var dayBookings =
                recentBookings.Count(b =>
                    b.BookingDate >= date &&
                    b.BookingDate < nextDate);

            var dayRevenue =
                recentBookings
                    .Where(b =>
                        b.BookingDate >= date &&
                        b.BookingDate < nextDate &&
                        b.Status == "Confirmed")
                    .Sum(b => b.TotalAmount);

            last7Days.Add(new
            {
                Date = date.ToString("dd/MM"),
                BookingCount = dayBookings,
                Revenue = dayRevenue
            });
        }


        // =========================
        // VIEWBAG
        // =========================

        ViewBag.TotalCustomers = totalCustomers;
        ViewBag.TotalUsers = totalUsers;

        ViewBag.TotalBusCompanies =
            totalBusCompanies;

        ViewBag.TotalBuses =
            totalBuses;

        ViewBag.TotalRoutes =
            totalRoutes;

        ViewBag.TotalTrips =
            totalTrips;

        ViewBag.TotalBookings =
            totalBookings;

        ViewBag.ConfirmedBookings =
            confirmedBookings;

        ViewBag.CancelledBookings =
            cancelledBookings;

        ViewBag.TotalRevenue =
            totalRevenue;

        ViewBag.ConfirmedRevenue =
            confirmedRevenue;

        ViewBag.CancelledRevenue =
            cancelledRevenue;

        ViewBag.TodayBookings =
            todayBookings;

        ViewBag.TodayRevenue =
            todayRevenue;

        ViewBag.TodayTrips =
            todayTrips;

        ViewBag.TotalSeats =
            totalSeats;

        ViewBag.BookedSeats =
            bookedSeats;

        ViewBag.AvailableSeats =
            availableSeats;

        ViewBag.TopBusCompanies =
            topBusCompanies;

        ViewBag.TopRoutes =
            topRoutes;

        ViewBag.Last7Days =
            last7Days;

        return View();
    }
}