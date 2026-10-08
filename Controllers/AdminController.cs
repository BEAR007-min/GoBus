using GoBus.Data;
using GoBus.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminController(GoBusDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // ADMIN DASHBOARD
    // GET: /Admin
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var model = new AdminDashboardViewModel
        {
            // Người dùng
            TotalCustomers = await _context.Customers
                .CountAsync(),

            TotalUsers = await _context.Users
                .CountAsync(),

            // Nhà xe
            TotalBusCompanies = await _context.BusCompanies
                .CountAsync(),

            // Xe
            TotalBuses = await _context.Buses
                .CountAsync(),

            // Tuyến
            TotalRoutes = await _context.Routes
                .CountAsync(),

            // Chuyến
            TotalTrips = await _context.Trips
                .CountAsync(),

            TodayTrips = await _context.Trips
                .CountAsync(t =>
                    t.DepartureTime >= today &&
                    t.DepartureTime < tomorrow),

            // Vé
            TotalBookings = await _context.Bookings
                .CountAsync(),

            ConfirmedBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed"),

            CancelledBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Cancelled"),

            // Doanh thu
            TotalRevenue = await _context.Bookings
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0,

            ConfirmedRevenue = await _context.Bookings
                .Where(b => b.Status == "Confirmed")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0
        };

        return View(model);
    }
}