using GoBus.Data;
using GoBus.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoBus.Controllers;

public class ForgotPasswordController : Controller
{
    private readonly GoBusDbContext _context;
    private readonly OtpService _otpService;
    private readonly PasswordService _passwordService;
    private readonly IEmailService _emailService;

    public ForgotPasswordController(
    GoBusDbContext context,
    OtpService otpService,
    PasswordService passwordService,
    IEmailService emailService)
{
    _context = context;
    _otpService = otpService;
    _passwordService = passwordService;
    _emailService = emailService;
}

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string emailOrPhone)
    {
        if (string.IsNullOrWhiteSpace(emailOrPhone))
        {
            ModelState.AddModelError(
                "emailOrPhone",
                "Vui lòng nhập Email hoặc số điện thoại.");

            return View();
        }

        emailOrPhone = emailOrPhone.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == emailOrPhone ||
                u.Phone == emailOrPhone);

        if (user == null)
        {
            ModelState.AddModelError(
                "emailOrPhone",
                "Không tìm thấy tài khoản với Email hoặc số điện thoại này.");

            return View();
        }

        // Tạo OTP
        string otp = _otpService.GenerateOtp(user.Id);

        // Lưu UserId để bước xác thực OTP sử dụng.
        TempData["ForgotPasswordUserId"] = user.Id;

        // ============================================
        // TẠM THỜI CHỈ DÙNG ĐỂ TEST
        // Sau này thay bằng Email/SMS.
        // ============================================

        TempData["DevOtp"] = otp;

        return RedirectToAction("VerifyOtp");
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        if (TempData["ForgotPasswordUserId"] == null)
        {
            return RedirectToAction("Index");
        }

        TempData.Keep("ForgotPasswordUserId");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyOtp(string otp)
    {
        if (TempData["ForgotPasswordUserId"] == null)
        {
            return RedirectToAction("Index");
        }

        if (!int.TryParse(
                TempData["ForgotPasswordUserId"]?.ToString(),
                out int userId))
        {
            return RedirectToAction("Index");
        }

        TempData.Keep("ForgotPasswordUserId");

        if (string.IsNullOrWhiteSpace(otp))
        {
            ModelState.AddModelError(
                "otp",
                "Vui lòng nhập mã OTP.");

            return View();
        }

        otp = otp.Trim();

        if (otp.Length != 6 ||
            !otp.All(char.IsDigit))
        {
            ModelState.AddModelError(
                "otp",
                "OTP phải gồm đúng 6 chữ số.");

            return View();
        }

        bool isValid = _otpService.VerifyOtp(userId, otp);

        if (!isValid)
        {
            ModelState.AddModelError(
                "otp",
                "OTP không đúng, đã hết hạn hoặc đã vượt quá số lần thử.");

            return View();
        }

        // OTP đúng → cho phép sang bước đổi mật khẩu.
        TempData["ResetPasswordUserId"] = userId;

        return RedirectToAction("ResetPassword");
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(
    string newPassword,
    string confirmPassword)
{
    if (TempData["ResetPasswordUserId"] == null)
    {
        return RedirectToAction("Index");
    }

    if (!int.TryParse(
            TempData["ResetPasswordUserId"]?.ToString(),
            out int userId))
    {
        return RedirectToAction("Index");
    }

    TempData.Keep("ResetPasswordUserId");

    if (string.IsNullOrWhiteSpace(newPassword))
    {
        ModelState.AddModelError(
            "newPassword",
            "Vui lòng nhập mật khẩu mới.");

        return View();
    }

    if (newPassword.Length < 6)
    {
        ModelState.AddModelError(
            "newPassword",
            "Mật khẩu phải có ít nhất 6 ký tự.");

        return View();
    }

    if (newPassword != confirmPassword)
    {
        ModelState.AddModelError(
            "confirmPassword",
            "Mật khẩu xác nhận không khớp.");

        return View();
    }

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
    {
        return NotFound("Không tìm thấy tài khoản.");
    }

    user.Password = _passwordService.HashPassword(newPassword);

    await _context.SaveChangesAsync();

    TempData.Remove("ResetPasswordUserId");

    return RedirectToAction(
        "Login",
        "Account",
        new
        {
            resetSuccess = true
        });
}

    [HttpGet]
    public IActionResult ResetPassword()
    {
        if (TempData["ResetPasswordUserId"] == null)
        {
            return RedirectToAction("Index");
        }

        TempData.Keep("ResetPasswordUserId");

        return View();
    }
}