using GoBus.Models;

namespace GoBus.Data;

using GoBusRoute = GoBus.Models.Route;
public static class DbInitializer
{
    public static void Initialize(GoBusDbContext context)
    {
        context.Database.EnsureCreated();

        // ==========================================
        // 1. TẠO NHÀ XE
        // ==========================================

        SeedBusCompanies(context);

        // ==========================================
        // 2. TẠO XE
        // ==========================================

        SeedBuses(context);

        // ==========================================
        // 3. TẠO TUYẾN ĐƯỜNG
        // ==========================================

        SeedRoutes(context);

        // ==========================================
        // 4. TẠO CHUYẾN XE
        // ==========================================

        SeedTrips(context);
    }


    // =========================================================
    // NHÀ XE
    // =========================================================

    private static void SeedBusCompanies(GoBusDbContext context)
    {
        if (context.BusCompanies.Any())
        {
            return;
        }

        var companies = new List<BusCompany>
        {
            new BusCompany
            {
                Name = "Hoàng Long",
                Phone = "19006789",
                Email = "contact@hoanglong.vn",
                Address = "Hà Nội",
                Description = "Nhà xe phục vụ các tuyến Bắc - Trung - Nam."
            },

            new BusCompany
            {
                Name = "Phương Trang",
                Phone = "19006067",
                Email = "contact@futabus.vn",
                Address = "TP. Hồ Chí Minh",
                Description = "Nhà xe Phương Trang."
            },

            new BusCompany
            {
                Name = "Mai Linh",
                Phone = "19006789",
                Email = "contact@mailinh.vn",
                Address = "TP. Hồ Chí Minh",
                Description = "Nhà xe Mai Linh."
            },

            new BusCompany
            {
                Name = "Kumho Samco",
                Phone = "19006079",
                Email = "contact@kumhosamco.vn",
                Address = "TP. Hồ Chí Minh",
                Description = "Nhà xe Kumho Samco."
            },

            new BusCompany
            {
                Name = "Hà Sơn Hải Vân",
                Phone = "19006776",
                Email = "contact@hasonhaivan.com",
                Address = "Lào Cai",
                Description = "Nhà xe Hà Sơn Hải Vân."
            },

            new BusCompany
            {
                Name = "Cúc Tùng",
                Phone = "19001010",
                Email = "contact@cuctung.vn",
                Address = "Khánh Hòa",
                Description = "Nhà xe Cúc Tùng."
            },

            new BusCompany
            {
                Name = "Thành Bưởi",
                Phone = "19006079",
                Email = "contact@thanhbuoi.com.vn",
                Address = "TP. Hồ Chí Minh",
                Description = "Nhà xe Thành Bưởi."
            },

            new BusCompany
            {
                Name = "Quốc Đạt",
                Phone = "19009090",
                Email = "contact@quocdat.vn",
                Address = "Đà Nẵng",
                Description = "Nhà xe Quốc Đạt."
            },

            new BusCompany
            {
                Name = "Tân Niên",
                Phone = "19008888",
                Email = "contact@tannien.vn",
                Address = "Đồng Nai",
                Description = "Nhà xe Tân Niên."
            },

            new BusCompany
            {
                Name = "Liên Hưng",
                Phone = "19007777",
                Email = "contact@lienhung.vn",
                Address = "Khánh Hòa",
                Description = "Nhà xe Liên Hưng."
            }
        };

        context.BusCompanies.AddRange(companies);
        context.SaveChanges();
    }


    // =========================================================
    // XE
    // =========================================================

    private static void SeedBuses(GoBusDbContext context)
    {
        if (context.Buses.Any())
        {
            return;
        }

        var companies = context.BusCompanies
            .OrderBy(c => c.Id)
            .ToList();

        if (companies.Count == 0)
        {
            return;
        }

        var buses = new List<Bus>();

        int busNumber = 1;

        foreach (var company in companies)
        {
            // Mỗi nhà xe có 5 xe
            for (int i = 1; i <= 5; i++)
            {
                bool isSleeper = i % 2 == 0;

                buses.Add(new Bus
                {
                    BusCompanyId = company.Id,

                    LicensePlate = $"29A-{10000 + busNumber}",

                    BusType = isSleeper
                        ? "Xe giường nằm 24 chỗ"
                        : "Xe ghế ngồi 45 chỗ",

                    TotalSeats = isSleeper
                        ? 24
                        : 45
                });

                busNumber++;
            }
        }

        context.Buses.AddRange(buses);
        context.SaveChanges();
    }


    // =========================================================
    // TUYẾN ĐƯỜNG
    // =========================================================

    private static void SeedRoutes(GoBusDbContext context)
    {
        if (context.Routes.Any())
        {
            return;
        }

        /*
         * Danh sách địa điểm đang sử dụng trong GoBus.
         *
         * Bạn có thể giữ danh sách 63 tỉnh/thành hiện tại
         * mà dự án của bạn đang sử dụng.
         */

        var provinces = new List<string>
        {
            "Hà Nội",
            "Hà Giang",
            "Cao Bằng",
            "Bắc Kạn",
            "Tuyên Quang",
            "Lào Cai",
            "Điện Biên",
            "Lai Châu",
            "Sơn La",
            "Yên Bái",
            "Hòa Bình",
            "Thái Nguyên",
            "Lạng Sơn",
            "Quảng Ninh",
            "Bắc Giang",
            "Phú Thọ",
            "Vĩnh Phúc",
            "Bắc Ninh",
            "Hải Dương",
            "Hải Phòng",
            "Hưng Yên",
            "Thái Bình",
            "Hà Nam",
            "Nam Định",
            "Ninh Bình",
            "Thanh Hóa",
            "Nghệ An",
            "Hà Tĩnh",
            "Quảng Bình",
            "Quảng Trị",
            "Thừa Thiên Huế",
            "Đà Nẵng",
            "Quảng Nam",
            "Quảng Ngãi",
            "Bình Định",
            "Phú Yên",
            "Khánh Hòa",
            "Ninh Thuận",
            "Bình Thuận",
            "Kon Tum",
            "Gia Lai",
            "Đắk Lắk",
            "Đắk Nông",
            "Lâm Đồng",
            "Bình Phước",
            "Tây Ninh",
            "Bình Dương",
            "Đồng Nai",
            "Bà Rịa - Vũng Tàu",
            "TP. Hồ Chí Minh",
            "Long An",
            "Tiền Giang",
            "Bến Tre",
            "Trà Vinh",
            "Vĩnh Long",
            "Đồng Tháp",
            "An Giang",
            "Kiên Giang",
            "Cần Thơ",
            "Hậu Giang",
            "Sóc Trăng",
            "Bạc Liêu",
            "Cà Mau"
        };

        var routes = new List<GoBusRoute>();

        var random = new Random(2026);

        /*
         * Tạo tất cả tuyến A → B.
         *
         * Ví dụ:
         * Hà Nội → Đà Nẵng
         * Đà Nẵng → Hà Nội
         *
         * Không tạo:
         * Hà Nội → Hà Nội
         */

        foreach (var departure in provinces)
        {
            foreach (var destination in provinces)
            {
                if (departure == destination)
                {
                    continue;
                }

                double distance = random.Next(50, 1501);

                int duration = random.Next(90, 1801);

                routes.Add(new GoBusRoute
            {
                Departure = departure,
                Destination = destination,
                Distance = distance,
                EstimatedDuration = duration
            });
            }
        }

        context.Routes.AddRange(routes);
        context.SaveChanges();
    }


    // =========================================================
    // CHUYẾN XE
    // =========================================================

    private static void SeedTrips(GoBusDbContext context)
    {
        /*
         * Nếu database đã có chuyến thì không tạo lại.
         */

        if (context.Trips.Any())
        {
            return;
        }

        var companies = context.BusCompanies
            .OrderBy(c => c.Id)
            .ToList();

        var buses = context.Buses
            .OrderBy(b => b.Id)
            .ToList();

        var routes = context.Routes
            .OrderBy(r => r.Id)
            .ToList();

        if (!companies.Any() ||
            !buses.Any() ||
            !routes.Any())
        {
            return;
        }

        var random = new Random(2026);

        /*
         * Các giờ khởi hành.
         */

        int[] departureHours =
        {
            5,
            7,
            9,
            11,
            13,
            15,
            17,
            19,
            21
        };

        int[] minutes =
        {
            0,
            15,
            30,
            45
        };

        /*
         * Tạo chuyến trong 365 ngày.
         */

        DateTime startDate = DateTime.Today;

        /*
         * Vì số lượng chuyến rất lớn,
         * không lưu toàn bộ vào RAM cùng lúc.
         */

        const int batchSize = 5000;

        var tripBatch = new List<Trip>(batchSize);

        int busIndex = 0;

        for (int day = 0; day < 365; day++)
        {
            DateTime currentDate = startDate.AddDays(day);

            foreach (var route in routes)
            {
                /*
                 * Mỗi tuyến có 2 chuyến/ngày.
                 */

                for (int tripNumber = 0;
                     tripNumber < 2;
                     tripNumber++)
                {
                    var bus = buses[busIndex % buses.Count];

                    busIndex++;

                    int hour =
                        departureHours[
                            random.Next(departureHours.Length)
                        ];

                    int minute =
                        minutes[
                            random.Next(minutes.Length)
                        ];

                    DateTime departureTime =
                        currentDate
                            .Date
                            .AddHours(hour)
                            .AddMinutes(minute);

                    int duration =
                        route.EstimatedDuration;

                    DateTime arrivalTime =
                        departureTime.AddMinutes(duration);

                    decimal price =
                        120000 +
                        random.Next(0, 9) * 50000;

                    int availableSeats =
                        bus.TotalSeats;

                    tripBatch.Add(new Trip
                    {
                        BusCompanyId = bus.BusCompanyId,

                        BusId = bus.Id,

                        RouteId = route.Id,

                        DepartureTime = departureTime,

                        ArrivalTime = arrivalTime,

                        Price = price,

                        AvailableSeats = availableSeats
                    });

                    /*
                     * Khi đủ 5.000 chuyến thì lưu xuống DB.
                     */

                    if (tripBatch.Count >= batchSize)
                    {
                        context.Trips.AddRange(tripBatch);

                        context.SaveChanges();

                        tripBatch.Clear();

                        Console.WriteLine(
                            $"Đã tạo chuyến đến ngày {currentDate:dd/MM/yyyy}"
                        );
                    }
                }
            }
        }

        /*
         * Lưu phần còn lại.
         */

        if (tripBatch.Count > 0)
        {
            context.Trips.AddRange(tripBatch);

            context.SaveChanges();

            tripBatch.Clear();
        }
    }
}