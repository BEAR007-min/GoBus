using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBusesController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminBusesController(GoBusDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        int? busCompanyId)
    {
        IQueryable<Bus> query = _context.Buses
            .Include(b => b.BusCompany)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(b =>
                b.LicensePlate.Contains(search) ||
                b.BusType.Contains(search));
        }

        if (busCompanyId.HasValue)
        {
            query = query.Where(b =>
                b.BusCompanyId == busCompanyId.Value);
        }

        var buses = await query
            .OrderBy(b => b.BusCompany!.Name)
            .ThenBy(b => b.LicensePlate)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.BusCompanyId = busCompanyId;

        ViewBag.BusCompanies = await _context.BusCompanies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(buses);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var bus = await _context.Buses
            .Include(b => b.BusCompany)
            .Include(b => b.Trips)
                .ThenInclude(t => t.Route)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null)
        {
            return NotFound();
        }

        return View(bus);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.BusCompanies = await _context.BusCompanies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bus model)
    {
        if (!await _context.BusCompanies
            .AnyAsync(c => c.Id == model.BusCompanyId))
        {
            ModelState.AddModelError(
                "BusCompanyId",
                "Nhà xe không tồn tại.");
        }

        if (await _context.Buses.AnyAsync(
            b => b.LicensePlate == model.LicensePlate))
        {
            ModelState.AddModelError(
                "LicensePlate",
                "Biển số xe đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusCompanies = await _context.BusCompanies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(model);
        }

        model.LicensePlate = model.LicensePlate.Trim();
        model.BusType = model.BusType.Trim();

        if (model.LayoutType == "Sleeper24")
        {
            model.BusType = "Xe giường nằm 24 chỗ";
            model.TotalSeats = 24;
        }
        else
        {
            model.LayoutType = "Seat45";
            model.BusType = "Xe ghế ngồi 45 chỗ";
            model.TotalSeats = 45;
        }

        _context.Buses.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã thêm xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null)
        {
            return NotFound();
        }

        ViewBag.BusCompanies = await _context.BusCompanies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(bus);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Bus model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        bool duplicateLicensePlate =
            await _context.Buses.AnyAsync(b =>
                b.Id != id &&
                b.LicensePlate == model.LicensePlate);

        if (duplicateLicensePlate)
        {
            ModelState.AddModelError(
                "LicensePlate",
                "Biển số xe đã tồn tại.");
        }

        if (!await _context.BusCompanies
            .AnyAsync(c => c.Id == model.BusCompanyId))
        {
            ModelState.AddModelError(
                "BusCompanyId",
                "Nhà xe không tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.BusCompanies = await _context.BusCompanies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(model);
        }

        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null)
        {
            return NotFound();
        }

        bus.BusCompanyId = model.BusCompanyId;
        bus.LicensePlate = model.LicensePlate.Trim();

        if (model.LayoutType == "Sleeper24")
        {
            bus.LayoutType = "Sleeper24";
            bus.BusType = "Xe giường nằm 24 chỗ";
            bus.TotalSeats = 24;
        }
        else
        {
            bus.LayoutType = "Seat45";
            bus.BusType = "Xe ghế ngồi 45 chỗ";
            bus.TotalSeats = 45;
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã cập nhật xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var bus = await _context.Buses
            .Include(b => b.Trips)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null)
        {
            return NotFound();
        }

        if (bus.Trips.Any())
        {
            TempData["ErrorMessage"] =
                "Không thể xóa xe vì xe đang được sử dụng trong chuyến xe.";

            return RedirectToAction(nameof(Index));
        }

        _context.Buses.Remove(bus);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã xóa xe thành công.";

        return RedirectToAction(nameof(Index));
    }
}