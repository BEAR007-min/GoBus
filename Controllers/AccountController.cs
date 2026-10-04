using GoBus.Data;
using GoBus.Models;
using GoBus.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GoBus.Controllers;

public class AccountController : Controller
{
    private readonly GoBusDbContext _context;

    public AccountController(GoBusDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool usernameExists = await _context.Users
            .AnyAsync(u => u.Username == model.Username);

        if (usernameExists)
        {
            ModelState.AddModelError(
                "Username",
                "Tên đăng nhập đã tồn tại.");

            return View(model);
        }

        bool phoneExists = await _context.Customers
            .AnyAsync(c => c.Phone == model.Phone);

        if (phoneExists)
        {
            ModelState.AddModelError(
                "Phone",
                "Số điện thoại đã được sử dụng.");

            return View(model);
        }

        var customer = new Customer
        {
            FullName = model.FullName,
            Phone = model.Phone,
            Email = model.Email
        };

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        var user = new User
        {
            Username = model.Username,
            Password = model.Password,
            Role = "Customer",
            CustomerId = customer.Id
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    // GET: /Account/Login
    [HttpGet]
public IActionResult Login()
{
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = await _context.Users
        .Include(u => u.Customer)
        .FirstOrDefaultAsync(u => u.Username == model.Username);

    if (user == null || user.Password != model.Password)
    {
        ModelState.AddModelError(
            "",
            "Tên đăng nhập hoặc mật khẩu không chính xác.");

        return View(model);
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

    if (user.CustomerId.HasValue)
    {
        claims.Add(
            new Claim(
                "CustomerId",
                user.CustomerId.Value.ToString()));
    }

    var identity = new ClaimsIdentity(
        claims,
        CookieAuthenticationDefaults.AuthenticationScheme);

    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = model.RememberMe
        });

    return RedirectToAction("Index", "Home");
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

    return RedirectToAction("Index", "Home");
}

}