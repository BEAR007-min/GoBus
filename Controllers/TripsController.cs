using GoBus.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

public class TripsController : Controller
{
    private readonly GoBusDbContext _context;

    public TripsController(GoBusDbContext context)
    {
        _context = context;
    }

public async Task<IActionResult> Details(int id)
{
    var trip = await _context.Trips
        .Include(t => t.Route)
        .Include(t => t.BusCompany)
        .Include(t => t.Bus)
        .AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == id);

    if (trip == null)
    {
        return NotFound();
    }

    return View(trip);
}

    // =====================================================
    // TÌM KIẾM CHUYẾN XE
    // =====================================================

    public async Task<IActionResult> Index(
        string? departure,
        string? destination,
        DateTime? date)
    {

        if (date.HasValue && date.Value.Date < DateTime.Today)
    {
        TempData["Error"] = "Ngày đi không được ở trong quá khứ.";
        return RedirectToAction("Index", "Home");
    }

        var query = _context.Trips
            .Include(t => t.Route)
            .Include(t => t.BusCompany)
            .Include(t => t.Bus)
            .AsNoTracking()
            .AsQueryable();

        // =================================================
        // 1. LỌC ĐIỂM ĐI
        // =================================================

        if (!string.IsNullOrWhiteSpace(departure))
        {
            query = query.Where(t =>
                t.Route != null &&
                t.Route.Departure == departure);
        }


        // =================================================
        // 2. LỌC ĐIỂM ĐẾN
        // =================================================

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(t =>
                t.Route != null &&
                t.Route.Destination == destination);
        }


        // =================================================
        // 3. LỌC NGÀY
        // =================================================

        if (date.HasValue)
        {
            DateTime startDate =
                date.Value.Date;

            DateTime endDate =
                startDate.AddDays(1);

            query = query.Where(t =>
                t.DepartureTime >= startDate &&
                t.DepartureTime < endDate);
        }


        // =================================================
        // 4. SẮP XẾP
        // =================================================

        query = query.OrderBy(
            t => t.DepartureTime);


        // =================================================
        // 5. GIỚI HẠN KẾT QUẢ
        // =================================================

        var trips = await query
            .Take(100)
            .ToListAsync();


        // =================================================
        // 6. TRẢ DỮ LIỆU CHO VIEW
        // =================================================

        return View(trips);
    }
}