using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

public class SeatsController : Controller
{
    private readonly GoBusDbContext _context;

    public SeatsController(GoBusDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // HIỂN THỊ SƠ ĐỒ GHẾ / GIƯỜNG
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(int tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.Route)
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound("Không tìm thấy chuyến xe.");
        }

        if (trip.Bus == null)
        {
            return BadRequest("Chuyến xe chưa được cấu hình xe.");
        }

        // -----------------------------------------------------
        // XÁC ĐỊNH LOẠI XE
        // -----------------------------------------------------
        //
        // Ưu tiên LayoutType:
        //
        // Sleeper24 -> 24 giường
        // Seat45    -> 45 ghế
        //
        // Nếu LayoutType chưa đúng thì kiểm tra BusType.
        // -----------------------------------------------------

        string layoutType = GetLayoutType(trip.Bus);

        // -----------------------------------------------------
        // NẾU CHƯA CÓ SEAT -> TẠO TỰ ĐỘNG
        // -----------------------------------------------------

        if (!trip.Seats.Any())
        {
            var seats = CreateSeats(trip, layoutType);

            if (seats.Count == 0)
            {
                return BadRequest(
                    $"Không thể tạo sơ đồ cho loại xe: {trip.Bus.BusType}");
            }

            await _context.Seats.AddRangeAsync(seats);

            await _context.SaveChangesAsync();

            trip.Seats = seats;
        }

        // -----------------------------------------------------
        // LẤY DANH SÁCH THEO ĐÚNG THỨ TỰ
        // -----------------------------------------------------

        List<Seat> seatsOrdered;

        if (layoutType == "Sleeper24")
        {
            // A01 -> A12
            // B01 -> B12

            seatsOrdered = trip.Seats
                .OrderBy(s => s.SeatNumber.StartsWith("A") ? 0 : 1)
                .ThenBy(s => s.SeatNumber)
                .ToList();
        }
        else
        {
            // 01 -> 45

            seatsOrdered = trip.Seats
                .OrderBy(s => s.SeatNumber)
                .ToList();
        }

        // -----------------------------------------------------
        // TRUYỀN DỮ LIỆU SANG VIEW
        // -----------------------------------------------------

        ViewBag.Trip = trip;

        ViewBag.LayoutType = layoutType;

        ViewBag.IsSleeper = layoutType == "Sleeper24";

        ViewBag.TotalPlaces =
            layoutType == "Sleeper24"
                ? 24
                : 45;

        return View(seatsOrdered);
    }


    // =========================================================
    // XÁC ĐỊNH LOẠI SƠ ĐỒ
    // =========================================================

    private string GetLayoutType(Bus bus)
    {
        // -----------------------------------------------------
        // 1. Ưu tiên LayoutType trong database
        // -----------------------------------------------------

        if (bus.LayoutType == "Sleeper24")
        {
            return "Sleeper24";
        }

        if (bus.LayoutType == "Seat45")
        {
            return "Seat45";
        }

        // -----------------------------------------------------
        // 2. Nếu LayoutType chưa hợp lệ
        //    thì kiểm tra BusType
        // -----------------------------------------------------

        string busType = bus.BusType
            .Trim()
            .ToLowerInvariant();

        if (
            busType.Contains("giường") ||
            busType.Contains("giuong") ||
            busType.Contains("sleeper")
        )
        {
            return "Sleeper24";
        }

        // -----------------------------------------------------
        // 3. Mặc định là xe ghế
        // -----------------------------------------------------

        return "Seat45";
    }


    // =========================================================
    // TẠO SƠ ĐỒ
    // =========================================================

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


    // =========================================================
    // XE GHẾ NGỒI 45 CHỖ
    // =========================================================

    private List<Seat> CreateSeat45Layout(Trip trip)
    {
        var seats = new List<Seat>();

        const int totalSeats = 45;

        for (int i = 1; i <= totalSeats; i++)
        {
            seats.Add(
                new Seat
                {
                    TripId = trip.Id,

                    SeatNumber = i.ToString("00"),

                    SeatType = "Seat",

                    IsBooked = false
                }
            );
        }

        return seats;
    }


    // =========================================================
    // XE GIƯỜNG NẰM 24 GIƯỜNG
    // =========================================================
    //
    // Tầng dưới:
    // A01 -> A12
    //
    // Tầng trên:
    // B01 -> B12
    //
    // Tổng:
    // 12 + 12 = 24
    // =========================================================

    private List<Seat> CreateSleeper24Layout(Trip trip)
    {
        var seats = new List<Seat>();

        const int bedsPerFloor = 12;

        for (int i = 1; i <= bedsPerFloor; i++)
        {
            // -------------------------------------------------
            // TẦNG DƯỚI
            // -------------------------------------------------

            seats.Add(
                new Seat
                {
                    TripId = trip.Id,

                    SeatNumber = $"A{i:00}",

                    SeatType = "Sleeper",

                    IsBooked = false
                }
            );

            // -------------------------------------------------
            // TẦNG TRÊN
            // -------------------------------------------------

            seats.Add(
                new Seat
                {
                    TripId = trip.Id,

                    SeatNumber = $"B{i:00}",

                    SeatType = "Sleeper",

                    IsBooked = false
                }
            );
        }

        return seats;
    }
}