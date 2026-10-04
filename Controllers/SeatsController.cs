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

    // GET: /Seats/Index?tripId=1
    public async Task<IActionResult> Index(int tripId)
    {
        // Lấy thông tin chuyến xe
        var trip = await _context.Trips
            .Include(t => t.Route)
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        // Không tìm thấy chuyến
        if (trip == null)
        {
            return NotFound();
        }

        // Nếu chuyến chưa có ghế thì tự tạo ghế
        if (!trip.Seats.Any())
        {
            if (trip.Bus == null || trip.Bus.TotalSeats <= 0)
            {
                return BadRequest("Xe chưa được cấu hình số lượng ghế.");
            }

            var seats = new List<Seat>();

            int totalSeats = trip.Bus.TotalSeats;

            for (int i = 1; i <= totalSeats; i++)
            {
                // 4 ghế mỗi hàng
                int rowIndex = (i - 1) / 4;
                int position = ((i - 1) % 4) + 1;

                // A, B, C, D...
                char row = (char)('A' + rowIndex);

                seats.Add(new Seat
                {
                    TripId = trip.Id,
                    SeatNumber = $"{row}{position:00}",
                    SeatType = "Normal",
                    IsBooked = false
                });
            }

            await _context.Seats.AddRangeAsync(seats);
            await _context.SaveChangesAsync();

            // Lấy lại danh sách ghế sau khi lưu
            trip.Seats = seats;
        }

        var orderedSeats = trip.Seats
            .OrderBy(s => s.SeatNumber)
            .ToList();

        ViewBag.Trip = trip;

        return View(orderedSeats);
    }
}