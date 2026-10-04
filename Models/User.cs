namespace GoBus.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }
}