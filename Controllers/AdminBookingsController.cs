using GoBus.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBookingsController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminBookingsController(GoBusDbContext context)
    {
        _context = context;
    }

    // GET: /AdminBookings
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? status)
    {
        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Trip)
                .ThenInclude(t => t!.Route)
            .Include(b => b.Trip)
                .ThenInclude(t => t!.BusCompany)
            .Include(b => b.Trip)
                .ThenInclude(t => t!.Bus)
            .Include(b => b.BookingDetails)
                .ThenInclude(d => d.Seat)
            .AsNoTracking()
            .AsQueryable();

        // =========================
        // TÌM KIẾM
        // =========================

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(b =>
                (b.Customer != null &&
                 b.Customer.FullName.Contains(search)) ||

                (b.Customer != null &&
                 b.Customer.Phone.Contains(search)) ||

                (b.Customer != null &&
                 b.Customer.Email.Contains(search)) ||

                (b.Trip != null &&
                 b.Trip.Route != null &&
                 b.Trip.Route.Departure.Contains(search)) ||

                (b.Trip != null &&
                 b.Trip.Route != null &&
                 b.Trip.Route.Destination.Contains(search)) ||

                (b.Trip != null &&
                 b.Trip.BusCompany != null &&
                 b.Trip.BusCompany.Name.Contains(search)) ||

                (b.Trip != null &&
                 b.Trip.Bus != null &&
                 b.Trip.Bus.LicensePlate.Contains(search))
            );
        }

        // =========================
        // TÌM THEO MÃ VÉ
        // =========================

        if (int.TryParse(search, out int bookingId))
        {
            query = query.Where(b => b.Id == bookingId);
        }

        // =========================
        // LỌC TRẠNG THÁI
        // =========================

        if (!string.IsNullOrWhiteSpace(status) &&
            status != "All")
        {
            query = query.Where(b => b.Status == status);
        }

        // =========================
        // LẤY DANH SÁCH
        // =========================

        var bookings = await query
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status ?? "All";

        return View(bookings);
    }

    // GET: /AdminBookings/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Trip)
                .ThenInclude(t => t!.Route)
            .Include(b => b.Trip)
                .ThenInclude(t => t!.BusCompany)
            .Include(b => b.Trip)
                .ThenInclude(t => t!.Bus)
            .Include(b => b.BookingDetails)
                .ThenInclude(d => d.Seat)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    // POST: /AdminBookings/Cancel
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t!.Bus)
            .Include(b => b.BookingDetails)
                .ThenInclude(d => d.Seat)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        // =========================
        // KIỂM TRA VÉ ĐÃ HỦY
        // =========================

        if (booking.Status == "Cancelled")
        {
            TempData["ErrorMessage"] =
                "Vé này đã được hủy trước đó.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================
        // KIỂM TRA CHUYẾN XE
        // =========================

        if (booking.Trip == null)
        {
            TempData["ErrorMessage"] =
                "Không tìm thấy chuyến xe của vé.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================
        // TRẢ GHẾ / GIƯỜNG
        // =========================

        foreach (var detail in booking.BookingDetails)
        {
            if (detail.Seat != null)
            {
                detail.Seat.IsBooked = false;
            }
        }

        // =========================
        // CẬP NHẬT SỐ GHẾ TRỐNG
        // =========================

        booking.Trip.AvailableSeats +=
            booking.BookingDetails.Count;

        // =========================
        // GIỚI HẠN THEO TỔNG SỐ GHẾ
        // =========================

        if (booking.Trip.Bus != null)
        {
            int totalSeats = booking.Trip.Bus.TotalSeats;

            if (booking.Trip.AvailableSeats > totalSeats)
            {
                booking.Trip.AvailableSeats = totalSeats;
            }
        }

        // =========================
        // CẬP NHẬT TRẠNG THÁI
        // =========================

        booking.Status = "Cancelled";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã hủy vé thành công và cập nhật lại số ghế trống.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}