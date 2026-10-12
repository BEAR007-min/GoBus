namespace GoBus.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    // Thông tin liên hệ dùng cho đăng nhập / khôi phục mật khẩu
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }
}