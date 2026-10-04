using GoBus.Models;

namespace GoBus.Data;

public static class DbInitializer
{
    public static void Initialize(GoBusDbContext context)
    {
        // Nếu đã có dữ liệu thì không seed lại
        if (context.BusCompanies.Any())
        {
            return;
        }

        // =====================================================
        // 1. NHÀ XE
        // =====================================================

        var companies = new List<BusCompany>
        {
            new BusCompany
            {
                Name = "GoBus Express",
                Phone = "0901000001",
                Email = "gobus1@gmail.com",
                Address = "Hà Nội",
                Description = "Nhà xe mô phỏng GoBus"
            },

            new BusCompany
            {
                Name = "Việt Lộ Coach",
                Phone = "0901000002",
                Email = "vietlo@gmail.com",
                Address = "Hà Nội",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Đại Việt Coach",
                Phone = "0901000003",
                Email = "daiviet@gmail.com",
                Address = "Hải Phòng",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Sông Hồng Coach",
                Phone = "0901000004",
                Email = "songhong@gmail.com",
                Address = "Hà Nội",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Trường Sơn Travel",
                Phone = "0901000005",
                Email = "truongson@gmail.com",
                Address = "Thanh Hóa",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Biển Việt",
                Phone = "0901000006",
                Email = "bienviet@gmail.com",
                Address = "Đà Nẵng",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Hoa Sen Coach",
                Phone = "0901000007",
                Email = "hoasen@gmail.com",
                Address = "TP. Hồ Chí Minh",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "An Phú Travel",
                Phone = "0901000008",
                Email = "anphu@gmail.com",
                Address = "Cần Thơ",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Phương Nam Coach",
                Phone = "0901000009",
                Email = "phuongnam@gmail.com",
                Address = "TP. Hồ Chí Minh",
                Description = "Nhà xe mô phỏng"
            },

            new BusCompany
            {
                Name = "Bắc Nam Express",
                Phone = "0901000010",
                Email = "bacnam@gmail.com",
                Address = "Hà Nội",
                Description = "Nhà xe mô phỏng"
            }
        };

        context.BusCompanies.AddRange(companies);
        context.SaveChanges();


        // =====================================================
        // 2. XE
        // =====================================================

        var buses = new List<Bus>();

        for (int i = 0; i < companies.Count; i++)
        {
            for (int j = 1; j <= 5; j++)
            {
                buses.Add(new Bus
                {
                    BusCompanyId = companies[i].Id,

                    LicensePlate =
                        $"29B-{10000 + i * 10 + j}",

                    BusType = j % 2 == 0
                        ? "Giường nằm"
                        : "Ghế ngồi",

                    TotalSeats = j % 2 == 0
                        ? 40
                        : 45
                });
            }
        }

        context.Buses.AddRange(buses);
        context.SaveChanges();


        // =====================================================
        // 3. 63 TỈNH / THÀNH
        // =====================================================

        string[] provinces =
        {
            "Hà Nội",
            "Hải Phòng",
            "Quảng Ninh",
            "Hà Giang",
            "Cao Bằng",
            "Bắc Kạn",
            "Tuyên Quang",
            "Lào Cai",
            "Yên Bái",
            "Thái Nguyên",
            "Lạng Sơn",
            "Bắc Giang",
            "Phú Thọ",
            "Vĩnh Phúc",
            "Bắc Ninh",
            "Hải Dương",
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


        // =====================================================
        // 4. TẠO TOÀN BỘ TUYẾN
        // =====================================================

        var routes = new List<GoBus.Models.Route>();

        /*
         * 63 tỉnh/thành
         *
         * Mỗi tỉnh có thể đi đến 62 tỉnh/thành còn lại.
         *
         * 63 × 62 = 3.906 tuyến một chiều
         */

        for (int departureIndex = 0;
             departureIndex < provinces.Length;
             departureIndex++)
        {
            for (int destinationIndex = 0;
                 destinationIndex < provinces.Length;
                 destinationIndex++)
            {
                // Không tạo tuyến tỉnh → chính nó
                if (departureIndex == destinationIndex)
                {
                    continue;
                }

                string departure =
                    provinces[departureIndex];

                string destination =
                    provinces[destinationIndex];


                // Khoảng cách mô phỏng
                double distance =
                    50 +
                    Math.Abs(destinationIndex - departureIndex) * 25;


                // Giới hạn khoảng cách
                if (distance > 1800)
                {
                    distance = 1800;
                }


                // Thời gian mô phỏng
                int estimatedDuration =
                    60 +
                    (int)(distance * 1.15);


                // Giới hạn thời gian
                if (estimatedDuration > 1800)
                {
                    estimatedDuration = 1800;
                }


                routes.Add(
                    new GoBus.Models.Route
                    {
                        Departure = departure,
                        Destination = destination,
                        Distance = distance,
                        EstimatedDuration =
                            estimatedDuration
                    }
                );
            }
        }


        context.Routes.AddRange(routes);
        context.SaveChanges();


        // =====================================================
        // 5. TẠO CHUYẾN XE
        // =====================================================

        var trips = new List<Trip>();

        var departureHours = new[]
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

        var random = new Random(2026);


        /*
         * 30 ngày
         *
         * Mỗi tuyến:
         * 3 chuyến/ngày
         *
         * 3.906 × 30 × 3
         *
         * = 351.540 chuyến
         */

        for (int day = 0; day < 365; day++)
        {
            DateTime date =
                DateTime.Today.AddDays(day);


            for (int routeIndex = 0;
                 routeIndex < routes.Count;
                 routeIndex++)
            {
                var route =
                    routes[routeIndex];


                // 2 chuyến/ngày
                for (int tripNumber = 0;
                     tripNumber < 2;
                     tripNumber++)
                {
                    // Chọn xe
                    var bus =
                        buses[
                            (routeIndex +
                             tripNumber +
                             day)
                            % buses.Count
                        ];


                    // Chọn giờ
                    int hour =
                        departureHours[
                            (routeIndex +
                             tripNumber)
                            % departureHours.Length
                        ];


                    // 00 / 15 / 30 / 45
                    int minute =
                        random.Next(0, 4) * 15;


                    DateTime departureTime =
                        date
                            .AddHours(hour)
                            .AddMinutes(minute);


                    DateTime arrivalTime =
                        departureTime.AddMinutes(
                            route.EstimatedDuration
                        );


                    // Giá vé mô phỏng
                    decimal price =
                        120000 +
                        random.Next(0, 9) * 50000;


                    trips.Add(
                        new Trip
                        {
                            BusCompanyId =
                                bus.BusCompanyId,

                            RouteId =
                                route.Id,

                            BusId =
                                bus.Id,

                            DepartureTime =
                                departureTime,

                            ArrivalTime =
                                arrivalTime,

                            Price =
                                price,

                            AvailableSeats =
                                bus.TotalSeats
                        }
                    );
                }
            }
        }


        context.Trips.AddRange(trips);
        context.SaveChanges();


        // =====================================================
        // 6. THÔNG BÁO
        // =====================================================

        Console.WriteLine(
            "=========================================="
        );

        Console.WriteLine(
            "       GoBus seed data completed!"
        );

        Console.WriteLine(
            "=========================================="
        );

        Console.WriteLine(
            $"Nhà xe       : {companies.Count}"
        );

        Console.WriteLine(
            $"Xe           : {buses.Count}"
        );

        Console.WriteLine(
            $"Tỉnh / thành : {provinces.Length}"
        );

        Console.WriteLine(
            $"Tuyến đường  : {routes.Count}"
        );

        Console.WriteLine(
            $"Chuyến xe    : {trips.Count}"
        );

        Console.WriteLine(
            "=========================================="
        );
    }
}