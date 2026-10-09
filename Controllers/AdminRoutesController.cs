using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminRoutesController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminRoutesController(GoBusDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        IQueryable<GoBus.Models.Route> query =
            _context.Routes
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(r =>
                r.Departure.Contains(search) ||
                r.Destination.Contains(search));
        }

        var routes = await query
            .OrderBy(r => r.Departure)
            .ThenBy(r => r.Destination)
            .ToListAsync();

        ViewBag.Search = search;

        return View(routes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var route = await _context.Routes
            .Include(r => r.Trips)
                .ThenInclude(t => t.BusCompany)
            .Include(r => r.Trips)
                .ThenInclude(t => t.Bus)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        return View(route);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        GoBus.Models.Route model)
    {
        model.Departure = model.Departure.Trim();
        model.Destination = model.Destination.Trim();

        if (string.Equals(
                model.Departure,
                model.Destination,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                "Destination",
                "Điểm đi và điểm đến không được giống nhau.");
        }

        bool exists = await _context.Routes.AnyAsync(r =>
            r.Departure == model.Departure &&
            r.Destination == model.Destination);

        if (exists)
        {
            ModelState.AddModelError(
                "",
                "Tuyến đường này đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Routes.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã thêm tuyến thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var route = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        return View(route);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        GoBus.Models.Route model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        model.Departure = model.Departure.Trim();
        model.Destination = model.Destination.Trim();

        if (string.Equals(
                model.Departure,
                model.Destination,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                "Destination",
                "Điểm đi và điểm đến không được giống nhau.");
        }

        bool exists = await _context.Routes.AnyAsync(r =>
            r.Id != id &&
            r.Departure == model.Departure &&
            r.Destination == model.Destination);

        if (exists)
        {
            ModelState.AddModelError(
                "",
                "Tuyến đường này đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var route = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        route.Departure = model.Departure;
        route.Destination = model.Destination;
        route.Distance = model.Distance;
        route.EstimatedDuration = model.EstimatedDuration;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã cập nhật tuyến thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _context.Routes
            .Include(r => r.Trips)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        if (route.Trips.Any())
        {
            TempData["ErrorMessage"] =
                "Không thể xóa tuyến này vì đang có chuyến xe sử dụng tuyến.";

            return RedirectToAction(nameof(Index));
        }

        _context.Routes.Remove(route);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã xóa tuyến thành công.";

        return RedirectToAction(nameof(Index));
    }
}