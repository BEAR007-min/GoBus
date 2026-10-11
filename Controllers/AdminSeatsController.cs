using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminSeatsController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminSeatsController(GoBusDbContext context)
    {
        _context = context;
    }

    // GET: /AdminSeats
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        DateTime? date)
    {
        IQueryable<Trip> query = _context.Trips
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Include(t => t.Seats)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(t =>
                (t.Route != null &&
                 (t.Route.Departure.Contains(search) ||
                  t.Route.Destination.Contains(search))) ||

                (t.BusCompany != null &&
                 t.BusCompany.Name.Contains(search)) ||

                (t.Bus != null &&
                 t.Bus.LicensePlate.Contains(search)));
        }

        if (date.HasValue)
        {
            var selectedDate = date.Value.Date;
            var nextDate = selectedDate.AddDays(1);

            query = query.Where(t =>
                t.DepartureTime >= selectedDate &&
                t.DepartureTime < nextDate);
        }

        var trips = await query
            .OrderBy(t => t.DepartureTime)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Date = date?.ToString("yyyy-MM-dd");

        return View(trips);
    }


    // GET: /AdminSeats/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound();
        }

        if (trip.Bus == null)
        {
            return BadRequest("Chuyến xe chưa được cấu hình xe.");
        }

        var layoutType = GetLayoutType(trip.Bus);

        // Nếu chuyến chưa có ghế/giường,
        // tự động tạo theo loại xe.
        if (!trip.Seats.Any())
        {
            var seats = CreateSeats(trip, layoutType);

            if (seats.Count == 0)
            {
                return BadRequest(
                    "Không thể tạo sơ đồ ghế/giường cho chuyến xe này.");
            }

            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();

            trip.Seats = seats;
        }

        var orderedSeats = layoutType == "Sleeper24"
            ? trip.Seats
                .OrderBy(s =>
                    s.SeatNumber.StartsWith("A") ? 0 : 1)
                .ThenBy(s => s.SeatNumber)
                .ToList()
            : trip.Seats
                .OrderBy(s => s.SeatNumber)
                .ToList();

        ViewBag.LayoutType = layoutType;
        ViewBag.IsSleeper = layoutType == "Sleeper24";

        ViewBag.TotalSeats = orderedSeats.Count;
        ViewBag.BookedSeats =
            orderedSeats.Count(s => s.IsBooked);

        ViewBag.AvailableSeats =
            orderedSeats.Count(s => !s.IsBooked);

        return View(orderedSeats);
    }


    // POST: /AdminSeats/Generate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.Bus)
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound();
        }

        if (trip.Bus == null)
        {
            TempData["ErrorMessage"] =
                "Chuyến xe chưa được cấu hình xe.";

            return RedirectToAction(nameof(Index));
        }

        if (trip.Seats.Any())
        {
            TempData["ErrorMessage"] =
                "Chuyến xe này đã có sơ đồ ghế/giường.";

            return RedirectToAction(
                nameof(Details),
                new { tripId });
        }

        var layoutType = GetLayoutType(trip.Bus);

        var seats = CreateSeats(trip, layoutType);

        if (seats.Count == 0)
        {
            TempData["ErrorMessage"] =
                "Không thể tạo sơ đồ ghế/giường.";

            return RedirectToAction(nameof(Index));
        }

        await _context.Seats.AddRangeAsync(seats);

        trip.AvailableSeats = seats.Count;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Đã tạo {seats.Count} ghế/giường cho chuyến xe.";

        return RedirectToAction(
            nameof(Details),
            new { tripId });
    }


    // POST: /AdminSeats/Reset
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(int tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.Bus)
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound();
        }

        var seatIds = trip.Seats
            .Select(s => s.Id)
            .ToList();

        if (seatIds.Count > 0)
        {
            var hasBooking =
                await _context.BookingDetails
                    .AnyAsync(bd =>
                        seatIds.Contains(bd.SeatId));

            if (hasBooking)
            {
                TempData["ErrorMessage"] =
                    "Không thể tạo lại sơ đồ vì đã có vé sử dụng ghế/giường của chuyến này.";

                return RedirectToAction(
                    nameof(Details),
                    new { tripId });
            }
        }

        if (trip.Seats.Any())
        {
            _context.Seats.RemoveRange(trip.Seats);
        }

        if (trip.Bus == null)
        {
            TempData["ErrorMessage"] =
                "Chuyến xe chưa được cấu hình xe.";

            return RedirectToAction(nameof(Index));
        }

        var layoutType = GetLayoutType(trip.Bus);

        var newSeats = CreateSeats(
            trip,
            layoutType);

        await _context.Seats.AddRangeAsync(newSeats);

        trip.AvailableSeats = newSeats.Count;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã tạo lại sơ đồ ghế/giường thành công.";

        return RedirectToAction(
            nameof(Details),
            new { tripId });
    }


    // POST: /AdminSeats/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        int tripId)
    {
        var seat = await _context.Seats
            .FirstOrDefaultAsync(s => s.Id == id);

        if (seat == null)
        {
            return NotFound();
        }

        if (seat.TripId != tripId)
        {
            return BadRequest();
        }

        if (seat.IsBooked)
        {
            TempData["ErrorMessage"] =
                "Không thể xóa ghế/giường đã được đặt.";

            return RedirectToAction(
                nameof(Details),
                new { tripId });
        }

        var hasBooking =
            await _context.BookingDetails
                .AnyAsync(bd => bd.SeatId == id);

        if (hasBooking)
        {
            TempData["ErrorMessage"] =
                "Không thể xóa ghế/giường đã từng được sử dụng trong vé.";

            return RedirectToAction(
                nameof(Details),
                new { tripId });
        }

        _context.Seats.Remove(seat);

        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip != null)
        {
            if (trip.AvailableSeats > 0)
            {
                trip.AvailableSeats--;
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã xóa ghế/giường thành công.";

        return RedirectToAction(
            nameof(Details),
            new { tripId });
    }


    // =========================
    // XÁC ĐỊNH LOẠI XE
    // =========================

    private string GetLayoutType(Bus bus)
    {
        if (bus.LayoutType == "Sleeper24")
        {
            return "Sleeper24";
        }

        if (bus.LayoutType == "Seat45")
        {
            return "Seat45";
        }

        var busType =
            bus.BusType
                .Trim()
                .ToLowerInvariant();

        if (busType.Contains("giường") ||
            busType.Contains("giuong") ||
            busType.Contains("sleeper"))
        {
            return "Sleeper24";
        }

        return "Seat45";
    }


    // =========================
    // TẠO SƠ ĐỒ
    // =========================

    private List<Seat> CreateSeats(
        Trip trip,
        string layoutType)
    {
        if (layoutType == "Sleeper24")
        {
            return CreateSleeper24Layout(trip);
        }

        return CreateSeat45Layout(trip);
    }


    // =========================
    // XE GHẾ NGỒI 45 CHỖ
    // =========================

    private List<Seat> CreateSeat45Layout(
        Trip trip)
    {
        var seats = new List<Seat>();

        const int totalSeats = 45;

        for (int i = 1; i <= totalSeats; i++)
        {
            seats.Add(new Seat
            {
                TripId = trip.Id,

                SeatNumber =
                    i.ToString("00"),

                SeatType = "Seat",

                IsBooked = false
            });
        }

        return seats;
    }


    // =========================
    // XE GIƯỜNG NẰM 24 CHỖ
    // =========================

    private List<Seat> CreateSleeper24Layout(
        Trip trip)
    {
        var seats = new List<Seat>();

        const int bedsPerFloor = 12;

        for (int i = 1; i <= bedsPerFloor; i++)
        {
            seats.Add(new Seat
            {
                TripId = trip.Id,

                SeatNumber =
                    $"A{i:00}",

                SeatType = "Sleeper",

                IsBooked = false
            });

            seats.Add(new Seat
            {
                TripId = trip.Id,

                SeatNumber =
                    $"B{i:00}",

                SeatType = "Sleeper",

                IsBooked = false
            });
        }

        return seats;
    }
}