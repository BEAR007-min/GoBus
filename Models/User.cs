namespace GoBus.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    // Lưu mật khẩu đã được hash, KHÔNG lưu mật khẩu gốc.
    // Tạm giữ tên Password để không phải thay đổi cấu trúc DB ngay.
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }
}