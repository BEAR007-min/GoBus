namespace GoBus.Models;

public class Bus
{
    public int Id { get; set; }

    public int BusCompanyId { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    // Tên loại xe
    // Ví dụ:
    // Xe ghế ngồi 45 chỗ
    // Xe giường nằm 24 chỗ
    public string BusType { get; set; } = string.Empty;

    // Tổng số chỗ trên xe
    public int TotalSeats { get; set; }

    // Loại sơ đồ của xe
    //
    // Seat45     = Ghế ngồi 45 chỗ
    // Sleeper24  = Giường nằm 24 chỗ
    //
    // Sau này có thể mở rộng:
    // Seat40
    // Sleeper34
    // Sleeper36
    public string LayoutType { get; set; } = "Seat45";

    // Nhà xe
    public BusCompany? BusCompany { get; set; }

    // Các chuyến sử dụng xe này
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}