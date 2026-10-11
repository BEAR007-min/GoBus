using GoBus.Data;
using GoBus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCustomersController : Controller
{
    private readonly GoBusDbContext _context;

    public AdminCustomersController(GoBusDbContext context)
    {
        _context = context;
    }

    // GET: /AdminCustomers
    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Customers
            .Include(c => c.Bookings)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(c =>
                c.FullName.Contains(search) ||
                c.Phone.Contains(search) ||
                c.Email.Contains(search));
        }

        var customers = await query
            .OrderBy(c => c.FullName)
            .ToListAsync();

        ViewBag.Search = search;

        return View(customers);
    }

    // GET: /AdminCustomers/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Trip)
                    .ThenInclude(t => t!.Route)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Trip)
                    .ThenInclude(t => t!.BusCompany)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.BookingDetails)
                    .ThenInclude(d => d.Seat)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.CustomerId == id);

        ViewBag.User = user;

        return View(customer);
    }

    // GET: /AdminCustomers/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

    // POST: /AdminCustomers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Customer model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        customer.FullName = model.FullName.Trim();
        customer.Phone = model.Phone.Trim();
        customer.Email = model.Email.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã cập nhật thông tin khách hàng thành công.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // POST: /AdminCustomers/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        // Không cho xóa khách hàng đã có lịch sử đặt vé
        if (customer.Bookings.Any())
        {
            TempData["ErrorMessage"] =
                "Không thể xóa khách hàng này vì khách hàng đã có lịch sử đặt vé.";

            return RedirectToAction(
                nameof(Index));
        }

        // Tìm tài khoản User liên kết
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.CustomerId == id);

        if (user != null)
        {
            _context.Users.Remove(user);
        }

        _context.Customers.Remove(customer);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Đã xóa khách hàng thành công.";

        return RedirectToAction(
            nameof(Index));
    }
}