using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBusCompaniesController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminBusCompaniesController(GoBusDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // DANH SÁCH
    // GET: /AdminBusCompanies
    // GET: /AdminBusCompanies?search=...
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        IQueryable<BusCompany> query = _context.BusCompanies
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Phone.Contains(search) ||
                c.Email.Contains(search) ||
                c.Address.Contains(search));
        }

        var companies = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Search = search;

        return View(companies);
    }

    // =========================================================
    // CHI TIẾT
    // GET: /AdminBusCompanies/Details/5
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var company = await _context.BusCompanies
            .Include(c => c.Buses)
            .Include(c => c.Trips)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return NotFound();
        }

        return View(company);
    }

    // =========================================================
    // THÊM
    // GET: /AdminBusCompanies/Create
    // =========================================================

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // =========================================================
    // THÊM
    // POST: /AdminBusCompanies/Create
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BusCompany model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Name = model.Name.Trim();
        model.Phone = model.Phone.Trim();
        model.Email = model.Email.Trim();
        model.Address = model.Address.Trim();
        model.Description = model.Description.Trim();

        _context.BusCompanies.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã thêm nhà xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // SỬA
    // GET: /AdminBusCompanies/Edit/5
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var company = await _context.BusCompanies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return NotFound();
        }

        return View(company);
    }

    // =========================================================
    // SỬA
    // POST: /AdminBusCompanies/Edit/5
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        BusCompany model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var company = await _context.BusCompanies
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return NotFound();
        }

        company.Name = model.Name.Trim();
        company.Phone = model.Phone.Trim();
        company.Email = model.Email.Trim();
        company.Address = model.Address.Trim();
        company.Description = model.Description.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã cập nhật nhà xe thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // XÓA
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var company = await _context.BusCompanies
            .Include(c => c.Buses)
            .Include(c => c.Trips)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
        {
            return NotFound();
        }

        // Không cho xóa nhà xe đang có dữ liệu liên quan.
        // Tránh làm mất dữ liệu xe/chuyến.
        if (company.Buses.Any() || company.Trips.Any())
        {
            TempData["ErrorMessage"] =
                "Không thể xóa nhà xe này vì đang có xe hoặc chuyến xe liên quan. " +
                "Hãy xử lý các xe/chuyến trước.";

            return RedirectToAction(nameof(Index));
        }

        _context.BusCompanies.Remove(company);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã xóa nhà xe thành công.";

        return RedirectToAction(nameof(Index));
    }
}