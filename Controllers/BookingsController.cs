using GoBus.Data;
using GoBus.Models;
using GoBus.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

public class BookingsController : Controller
{
    private readonly GoBusDbContext _context;

    public BookingsController(GoBusDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // TẠO ĐẶT VÉ - GET
    // /Bookings/Create?tripId=1&seatIds=1&seatIds=2
    // =========================================================
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create(
        int tripId,
        List<int> seatIds)
    {
        if (seatIds == null || seatIds.Count == 0)
        {
            return BadRequest("Bạn chưa chọn ghế.");
        }

        var trip = await _context.Trips
            .Include(t => t.Route)
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return NotFound("Không tìm thấy chuyến xe.");
        }

        var seats = await _context.Seats
            .Where(s =>
                s.TripId == tripId &&
                seatIds.Contains(s.Id))
            .OrderBy(s => s.SeatNumber)
            .ToListAsync();

        if (seats.Count != seatIds.Count)
        {
            return BadRequest("Một hoặc nhiều ghế không tồn tại.");
        }

        if (seats.Any(s => s.IsBooked))
        {
            return BadRequest(
                "Một hoặc nhiều ghế đã được đặt.");
        }

        ViewBag.Trip = trip;
        ViewBag.Seats = seats;

        var model = new BookingCreateViewModel
        {
            TripId = tripId,
            SeatIds = seatIds
        };

        return View(model);
    }


    // =========================================================
    // TẠO ĐẶT VÉ - POST
    // =========================================================
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        BookingCreateViewModel model)
    {
        // -----------------------------------------------------
        // Kiểm tra ghế
        // -----------------------------------------------------
        if (model.SeatIds == null ||
            model.SeatIds.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Bạn chưa chọn ghế.");

            await LoadBookingInformation(model);

            return View(model);
        }


        // -----------------------------------------------------
        // Lấy CustomerId từ tài khoản đang đăng nhập
        // -----------------------------------------------------
        var customerIdClaim =
            User.FindFirst("CustomerId")?.Value;

        if (!int.TryParse(
                customerIdClaim,
                out int customerId))
        {
            return Unauthorized();
        }


        // -----------------------------------------------------
        // Tìm Customer
        // -----------------------------------------------------
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
        {
            return Unauthorized();
        }


        // -----------------------------------------------------
        // Tìm chuyến xe
        // -----------------------------------------------------
        var trip = await _context.Trips
            .Include(t => t.Route)
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .FirstOrDefaultAsync(
                t => t.Id == model.TripId);

        if (trip == null)
        {
            return NotFound(
                "Không tìm thấy chuyến xe.");
        }


        // -----------------------------------------------------
        // Lấy các ghế được chọn
        // -----------------------------------------------------
        var seats = await _context.Seats
            .Where(s =>
                s.TripId == model.TripId &&
                model.SeatIds.Contains(s.Id))
            .OrderBy(s => s.SeatNumber)
            .ToListAsync();


        // -----------------------------------------------------
        // Kiểm tra số lượng ghế
        // -----------------------------------------------------
        if (seats.Count != model.SeatIds.Count)
        {
            return BadRequest(
                "Ghế không hợp lệ.");
        }


        // -----------------------------------------------------
        // Kiểm tra ghế đã được đặt chưa
        // -----------------------------------------------------
        if (seats.Any(s => s.IsBooked))
        {
            ModelState.AddModelError(
                "",
                "Một hoặc nhiều ghế vừa được người khác đặt. " +
                "Vui lòng chọn lại.");

            await LoadBookingInformation(model);

            return View(model);
        }


        // -----------------------------------------------------
        // Tính tổng tiền
        // -----------------------------------------------------
        decimal totalAmount =
            seats.Count * trip.Price;


        // -----------------------------------------------------
        // Tạo Booking
        // -----------------------------------------------------
        var booking = new Booking
        {
            CustomerId = customer.Id,
            TripId = trip.Id,
            BookingDate = DateTime.Now,
            TotalAmount = totalAmount,
            Status = "Confirmed"
        };

        _context.Bookings.Add(booking);


        // -----------------------------------------------------
        // Tạo BookingDetails
        // Đồng thời đánh dấu ghế đã đặt
        // -----------------------------------------------------
        foreach (var seat in seats)
        {
            var detail = new BookingDetail
            {
                Booking = booking,
                SeatId = seat.Id,
                Price = trip.Price
            };

            _context.BookingDetails.Add(detail);

            seat.IsBooked = true;
        }


        // -----------------------------------------------------
        // Cập nhật số ghế còn trống
        // -----------------------------------------------------
        trip.AvailableSeats = Math.Max(
            0,
            trip.AvailableSeats - seats.Count);


        // -----------------------------------------------------
        // Lưu database
        // -----------------------------------------------------
        await _context.SaveChangesAsync();


        // -----------------------------------------------------
        // Chuyển sang trang thành công
        // -----------------------------------------------------
        return RedirectToAction(
            nameof(Success),
            new { id = booking.Id });
    }


    // =========================================================
    // TRANG ĐẶT VÉ THÀNH CÔNG
    // =========================================================
    [HttpGet]
    public async Task<IActionResult> Success(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)

            .Include(b => b.Trip)
                .ThenInclude(t => t!.Route)

            .Include(b => b.Trip)
                .ThenInclude(t => t!.BusCompany)

            .Include(b => b.BookingDetails)
                .ThenInclude(d => d.Seat)

            .AsNoTracking()

            .FirstOrDefaultAsync(
                b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }


    // =========================================================
    // VÉ CỦA TÔI
    // =========================================================
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        // -----------------------------------------------------
        // Lấy CustomerId từ tài khoản đăng nhập
        // -----------------------------------------------------
        var customerIdClaim =
            User.FindFirst("CustomerId")?.Value;

        if (!int.TryParse(
                customerIdClaim,
                out int customerId))
        {
            return Unauthorized();
        }


        // -----------------------------------------------------
        // Lấy danh sách vé của Customer
        // -----------------------------------------------------
        var bookings = await _context.Bookings

            .Include(b => b.Customer)

            .Include(b => b.Trip)
                .ThenInclude(t => t!.Route)

            .Include(b => b.Trip)
                .ThenInclude(t => t!.BusCompany)

            .Include(b => b.BookingDetails)
                .ThenInclude(d => d.Seat)

            .Where(b => b.CustomerId == customerId)

            .OrderByDescending(
                b => b.BookingDate)

            .AsNoTracking()

            .ToListAsync();

        return View(bookings);
    }


    // =========================================================
    // HỦY VÉ
    // =========================================================
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        // -----------------------------------------------------
        // Lấy CustomerId đang đăng nhập
        // -----------------------------------------------------
        var customerIdClaim =
            User.FindFirst("CustomerId")?.Value;

        if (!int.TryParse(
                customerIdClaim,
                out int customerId))
        {
            return Unauthorized();
        }


        // -----------------------------------------------------
        // Tìm Booking
        // Chỉ cho phép hủy vé của chính mình
        // -----------------------------------------------------
        var booking = await _context.Bookings

            .Include(b => b.BookingDetails)

            .FirstOrDefaultAsync(
                b =>
                    b.Id == id &&
                    b.CustomerId == customerId);

        if (booking == null)
        {
            return NotFound(
                "Không tìm thấy vé.");
        }


        // -----------------------------------------------------
        // Kiểm tra vé đã hủy chưa
        // -----------------------------------------------------
        if (booking.Status == "Cancelled")
        {
            return BadRequest(
                "Vé này đã được hủy.");
        }


        // -----------------------------------------------------
        // Tìm chuyến xe
        // -----------------------------------------------------
        var trip = await _context.Trips
            .FirstOrDefaultAsync(
                t => t.Id == booking.TripId);

        if (trip == null)
        {
            return NotFound(
                "Không tìm thấy chuyến xe.");
        }


        // -----------------------------------------------------
        // Lấy danh sách SeatId
        // -----------------------------------------------------
        var seatIds = booking.BookingDetails
            .Select(d => d.SeatId)
            .ToList();


        // -----------------------------------------------------
        // Lấy các ghế
        // -----------------------------------------------------
        var seats = await _context.Seats
            .Where(s => seatIds.Contains(s.Id))
            .ToListAsync();


        // -----------------------------------------------------
        // Trả ghế về trạng thái chưa đặt
        // -----------------------------------------------------
        foreach (var seat in seats)
        {
            seat.IsBooked = false;
        }


        // -----------------------------------------------------
        // Cộng lại số ghế trống
        // -----------------------------------------------------
        trip.AvailableSeats += seats.Count;


        // -----------------------------------------------------
        // Cập nhật trạng thái Booking
        // -----------------------------------------------------
        booking.Status = "Cancelled";


        // -----------------------------------------------------
        // Lưu database
        // -----------------------------------------------------
        await _context.SaveChangesAsync();


        // -----------------------------------------------------
        // Quay lại Vé của tôi
        // -----------------------------------------------------
        return RedirectToAction(
            nameof(MyBookings));
    }


    // =========================================================
    // LOAD THÔNG TIN ĐẶT VÉ KHI CÓ LỖI
    // =========================================================
    private async Task LoadBookingInformation(
        BookingCreateViewModel model)
    {
        var trip = await _context.Trips

            .Include(t => t.Route)

            .Include(t => t.BusCompany)

            .Include(t => t.Bus)

            .FirstOrDefaultAsync(
                t => t.Id == model.TripId);


        var seats = await _context.Seats

            .Where(s =>
                s.TripId == model.TripId &&
                model.SeatIds.Contains(s.Id))

            .OrderBy(s => s.SeatNumber)

            .ToListAsync();


        ViewBag.Trip = trip;

        ViewBag.Seats = seats;
    }
}