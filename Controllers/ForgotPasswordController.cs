using GoBus.Data;
using GoBus.Models;
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

    // =========================================================
    // BƯỚC 1: QUÊN MẬT KHẨU
    // =========================================================

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string? emailOrPhone)
    {
        if (string.IsNullOrWhiteSpace(emailOrPhone))
        {
            ModelState.AddModelError(
                "emailOrPhone",
                "Vui lòng nhập Email hoặc số điện thoại.");

            return View();
        }

        emailOrPhone = emailOrPhone.Trim();

        // Chuẩn hóa dữ liệu người dùng nhập
        string inputPhone = NormalizePhone(emailOrPhone);
        string inputEmail = emailOrPhone.Trim();

        // =====================================================
        // TÌM USER
        // =====================================================

        // Lấy danh sách User để có thể chuẩn hóa Phone
        // tránh lỗi do DB chứa khoảng trắng / dấu cách / +84.
        var users = await _context.Users
            .ToListAsync();

        User? user = null;

        // -----------------------------------------------------
        // 1. Tìm bằng Email
        // -----------------------------------------------------

        user = users.FirstOrDefault(u =>
            !string.IsNullOrWhiteSpace(u.Email) &&
            u.Email.Trim()
                .Equals(
                    inputEmail,
                    StringComparison.OrdinalIgnoreCase));

        // -----------------------------------------------------
        // 2. Nếu chưa tìm thấy thì tìm bằng SĐT
        // -----------------------------------------------------

        if (user == null && !string.IsNullOrWhiteSpace(inputPhone))
        {
            user = users.FirstOrDefault(u =>
                NormalizePhone(u.Phone) == inputPhone);
        }

        // =====================================================
        // KHÔNG TÌM THẤY
        // =====================================================

        if (user == null)
        {
            ModelState.AddModelError(
                "emailOrPhone",
                "Không tìm thấy tài khoản với Email hoặc số điện thoại này.");

            return View();
        }

        // =====================================================
        // KIỂM TRA EMAIL
        // =====================================================

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError(
                "emailOrPhone",
                "Tài khoản chưa có Email để nhận OTP.");

            return View();
        }

        // =====================================================
        // TẠO OTP
        // =====================================================

        string otp = _otpService.GenerateOtp(user.Id);

        TempData["ForgotPasswordUserId"] = user.Id;

        // =====================================================
        // GỬI OTP QUA EMAIL
        // =====================================================

        try
        {
            await _emailService.SendOtpEmailAsync(
                user.Email,
                otp);
        }
        catch (Exception ex)
        {
            _otpService.RemoveOtp(user.Id);
            TempData.Remove("ForgotPasswordUserId");

            Console.WriteLine("========== OTP EMAIL ERROR ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("=====================================");

            ModelState.AddModelError(
                "emailOrPhone",
                "Lỗi gửi Email OTP: " + ex.Message);

            return View();
        }

        // Thành công
        return RedirectToAction(nameof(VerifyOtp));
    }

    // =========================================================
    // CHUẨN HÓA SỐ ĐIỆN THOẠI
    // =========================================================

    private static string NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Chỉ giữ lại chữ số
        string digits = new string(
            value.Where(char.IsDigit).ToArray());

        // +84xxxxxxxxx
        // sau khi bỏ dấu + sẽ thành 84xxxxxxxxx
        if (digits.StartsWith("84") && digits.Length == 11)
        {
            digits = "0" + digits[2..];
        }

        return digits;
    }

    // =========================================================
    // BƯỚC 2: NHẬP OTP
    // =========================================================

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        if (TempData["ForgotPasswordUserId"] == null)
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.Keep("ForgotPasswordUserId");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VerifyOtp(string? otp)
    {
        if (TempData["ForgotPasswordUserId"] == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!int.TryParse(
                TempData["ForgotPasswordUserId"]?.ToString(),
                out int userId))
        {
            return RedirectToAction(nameof(Index));
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

        if (otp.Length != 6 || !otp.All(char.IsDigit))
        {
            ModelState.AddModelError(
                "otp",
                "OTP phải gồm đúng 6 chữ số.");

            return View();
        }

        bool isValid = _otpService.VerifyOtp(
            userId,
            otp);

        if (!isValid)
        {
            ModelState.AddModelError(
                "otp",
                "OTP không đúng, đã hết hạn hoặc đã vượt quá số lần thử.");

            return View();
        }

        // OTP đúng
        TempData["ResetPasswordUserId"] = userId;

        TempData.Remove("ForgotPasswordUserId");

        return RedirectToAction(nameof(ResetPassword));
    }

    // =========================================================
    // BƯỚC 3: TRANG ĐỔI MẬT KHẨU
    // =========================================================

    [HttpGet]
    public IActionResult ResetPassword()
    {
        if (TempData["ResetPasswordUserId"] == null)
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.Keep("ResetPasswordUserId");

        return View();
    }

    // =========================================================
    // BƯỚC 3: XỬ LÝ ĐỔI MẬT KHẨU
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
        string? newPassword,
        string? confirmPassword)
    {
        if (TempData["ResetPasswordUserId"] == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!int.TryParse(
                TempData["ResetPasswordUserId"]?.ToString(),
                out int userId))
        {
            return RedirectToAction(nameof(Index));
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

        // Hash mật khẩu mới
        user.Password =
            _passwordService.HashPassword(newPassword);

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
}