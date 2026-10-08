using GoBus.Data;
using GoBus.Models;
using GoBus.Services;
using GoBus.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GoBus.Controllers;

public class AccountController : Controller
{
    private readonly GoBusDbContext _context;
    private readonly PasswordService _passwordService;

    public AccountController(
        GoBusDbContext context,
        PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    // =========================================================
    // REGISTER
    // GET: /Account/Register
    // =========================================================

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // =========================================================
    // REGISTER
    // POST: /Account/Register
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string username = model.Username.Trim();
        string phone = model.Phone.Trim();
        string email = model.Email.Trim();

        // Kiểm tra username
        bool usernameExists = await _context.Users
            .AnyAsync(u => u.Username == username);

        if (usernameExists)
        {
            ModelState.AddModelError(
                "Username",
                "Tên đăng nhập đã tồn tại.");

            return View(model);
        }

        // Kiểm tra số điện thoại
        bool phoneExists = await _context.Customers
            .AnyAsync(c => c.Phone == phone);

        if (phoneExists)
        {
            ModelState.AddModelError(
                "Phone",
                "Số điện thoại đã được sử dụng.");

            return View(model);
        }

        // Tạo Customer
        var customer = new Customer
        {
            FullName = model.FullName.Trim(),
            Phone = phone,
            Email = email
        };

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        // =====================================================
        // QUAN TRỌNG:
        // Không lưu password gốc.
        // Password được hash trước khi lưu database.
        // =====================================================

        var user = new User
        {
            Username = username,

            Password = _passwordService.HashPassword(
                model.Password),

            Role = "Customer",

            CustomerId = customer.Id
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    // =========================================================
    // LOGIN
    // GET: /Account/Login
    // =========================================================

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // =========================================================
    // LOGIN
    // POST: /Account/Login
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string username = model.Username.Trim();

        // Tìm tài khoản
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u =>
                u.Username == username);

        // Không tìm thấy username
        if (user == null)
        {
            ModelState.AddModelError(
                "",
                "Tên đăng nhập hoặc mật khẩu không đúng.");

            return View(model);
        }

        // =====================================================
        // KIỂM TRA PASSWORD HASH
        // =====================================================

        bool passwordCorrect =
            _passwordService.VerifyPassword(
                model.Password,
                user.Password);

        if (!passwordCorrect)
        {
            ModelState.AddModelError(
                "",
                "Tên đăng nhập hoặc mật khẩu không đúng.");

            return View(model);
        }

        // =====================================================
        // TẠO CLAIMS
        // =====================================================

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        // CustomerId chỉ tồn tại với tài khoản Customer
        if (user.CustomerId.HasValue)
        {
            claims.Add(
                new Claim(
                    "CustomerId",
                    user.CustomerId.Value.ToString()));
        }

        // =====================================================
        // TẠO COOKIE AUTHENTICATION
        // =====================================================

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        // =====================================================
        // ĐĂNG NHẬP THÀNH CÔNG
        // =====================================================

        if (user.Role == "Admin")
        {
            return RedirectToAction(
                "Index",
                "Admin");
        }

        return RedirectToAction(
            "Index",
            "Home");
    }

    // =========================================================
    // LOGOUT
    // POST: /Account/Logout
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            "Index",
            "Home");
    }

    // =========================================================
    // ACCESS DENIED
    // =========================================================

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}