using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminTripsController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminTripsController(GoBusDbContext context)
    {
        _context = context;
    }

    // =========================
    // DANH SÁCH CHUYẾN
    // =========================
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        DateTime? date)
    {
        IQueryable<Trip> query = _context.Trips
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(t =>
                t.Route!.Departure.Contains(search) ||
                t.Route.Destination.Contains(search) ||
                t.BusCompany!.Name.Contains(search) ||
                t.Bus!.LicensePlate.Contains(search));
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

    // =========================
    // CHI TIẾT
    // =========================
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var trip = await _context.Trips
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Include(t => t.Seats)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
        {
            return NotFound();
        }

        return View(trip);
    }

    // =========================
    // THÊM CHUYẾN - GET
    // =========================
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCreateData();

        return View();
    }

    // =========================
    // THÊM CHUYẾN - POST
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trip model)
    {
        if (model.ArrivalTime <= model.DepartureTime)
        {
            ModelState.AddModelError(
                "ArrivalTime",
                "Thời gian đến phải sau thời gian khởi hành.");
        }

        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == model.BusId);

        if (bus == null)
        {
            ModelState.AddModelError(
                "BusId",
                "Không tìm thấy xe.");
        }

        var company = await _context.BusCompanies
            .FirstOrDefaultAsync(c => c.Id == model.BusCompanyId);

        if (company == null)
        {
            ModelState.AddModelError(
                "BusCompanyId",
                "Không tìm thấy nhà xe.");
        }

        var route = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == model.RouteId);

        if (route == null)
        {
            ModelState.AddModelError(
                "RouteId",
                "Không tìm thấy tuyến.");
        }

        if (bus != null &&
            company != null &&
            bus.BusCompanyId != company.Id)
        {
            ModelState.AddModelError(
                "BusId",
                "Xe này không thuộc nhà xe đã chọn.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateData(
                model.BusCompanyId,
                model.BusId,
                model.RouteId);

            return View(model);
        }

        model.AvailableSeats = bus!.TotalSeats;

        _context.Trips.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã thêm chuyến xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // SỬA - GET
    // =========================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
        {
            return NotFound();
        }

        await LoadCreateData(
            trip.BusCompanyId,
            trip.BusId,
            trip.RouteId);

        return View(trip);
    }

    // =========================
    // SỬA - POST
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Trip model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (model.ArrivalTime <= model.DepartureTime)
        {
            ModelState.AddModelError(
                "ArrivalTime",
                "Thời gian đến phải sau thời gian khởi hành.");
        }

        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == model.BusId);

        if (bus == null)
        {
            ModelState.AddModelError(
                "BusId",
                "Không tìm thấy xe.");
        }

        var company = await _context.BusCompanies
            .FirstOrDefaultAsync(c => c.Id == model.BusCompanyId);

        if (company == null)
        {
            ModelState.AddModelError(
                "BusCompanyId",
                "Không tìm thấy nhà xe.");
        }

        var route = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == model.RouteId);

        if (route == null)
        {
            ModelState.AddModelError(
                "RouteId",
                "Không tìm thấy tuyến.");
        }

        if (bus != null &&
            company != null &&
            bus.BusCompanyId != company.Id)
        {
            ModelState.AddModelError(
                "BusId",
                "Xe này không thuộc nhà xe đã chọn.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateData(
                model.BusCompanyId,
                model.BusId,
                model.RouteId);

            return View(model);
        }

        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
        {
            return NotFound();
        }

        trip.BusCompanyId = model.BusCompanyId;
        trip.BusId = model.BusId;
        trip.RouteId = model.RouteId;
        trip.DepartureTime = model.DepartureTime;
        trip.ArrivalTime = model.ArrivalTime;
        trip.Price = model.Price;

        // Nếu chuyến chưa có booking,
        // đồng bộ lại số ghế theo xe mới.
        bool hasBooking = await _context.Bookings
            .AnyAsync(b => b.TripId == trip.Id);

        if (!hasBooking)
        {
            trip.AvailableSeats = bus!.TotalSeats;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã cập nhật chuyến xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // XÓA
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var trip = await _context.Trips
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
        {
            return NotFound();
        }

        bool hasBooking = await _context.Bookings
            .AnyAsync(b => b.TripId == id);

        if (hasBooking)
        {
            TempData["ErrorMessage"] =
                "Không thể xóa chuyến này vì đã có vé đặt.";

            return RedirectToAction(nameof(Index));
        }

        _context.Trips.Remove(trip);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã xóa chuyến xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // LOAD DROPDOWN
    // =========================
    private async Task LoadCreateData(
        int? selectedCompanyId = null,
        int? selectedBusId = null,
        int? selectedRouteId = null)
    {
        var companies = await _context.BusCompanies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        var buses = await _context.Buses
            .AsNoTracking()
            .Include(b => b.BusCompany)
            .OrderBy(b => b.BusCompany!.Name)
            .ThenBy(b => b.LicensePlate)
            .ToListAsync();

        var routes = await _context.Routes
            .AsNoTracking()
            .OrderBy(r => r.Departure)
            .ThenBy(r => r.Destination)
            .ToListAsync();

        ViewBag.BusCompanies = new SelectList(
            companies,
            "Id",
            "Name",
            selectedCompanyId);

        ViewBag.Buses = new SelectList(
            buses,
            "Id",
            "LicensePlate",
            selectedBusId);

        ViewBag.Routes = new SelectList(
            routes,
            "Id",
            "Id",
            selectedRouteId);

        ViewBag.RouteList = routes;
    }
}