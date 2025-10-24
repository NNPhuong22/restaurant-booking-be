using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;

namespace TableOrder_Hust.Services
{
    public interface IGoogleSheetsService
    {
        Task UpdateAllDashboardsAsync();
    }

    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly AppDbContext _context;
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string _spreadsheetId = "1B787tex8PBFqRlsnoqKPiT6qS6_CptcYtfzdXCpuQsU"; // Lấy từ URL của sheet
        private readonly string _serviceAccountKeyFile = "project-hust-473102-3e76cf8d2174.json";

        public GoogleSheetsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateAllDashboardsAsync()
        {
            var service = GetSheetsService();

            await UpdateReservationsByDayAsync(service);
            await UpdateReservationsByStatusAsync(service);
            await UpdateTopTablesAsync(service);
            await UpdatePartySizeAsync(service);
            await UpdateReservationsByBranchAsync(service);
        }

        // [Helper 1] - Xác thực và tạo service (Không đổi)
        private SheetsService GetSheetsService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(_serviceAccountKeyFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);
            }
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            });
        }

        // [Helper 2] - Hàm ghi dữ liệu (Không đổi)
        private async Task WriteToSheetAsync(SheetsService service, string range, List<IList<object>> values)
        {
            var valueRange = new ValueRange { Values = values };

            // 1. Xóa dữ liệu cũ
            var clearRequest = service.Spreadsheets.Values.Clear(new ClearValuesRequest(), _spreadsheetId, range);
            await clearRequest.ExecuteAsync();

            // 2. Ghi dữ liệu mới
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();
        }

        // [Chart 1] - Đặt bàn theo ngày (ĐÃ SỬA)
        private async Task UpdateReservationsByDayAsync(SheetsService service)
        {
            var startDate = DateTime.Today.AddDays(-6);

            // SỬA: Dùng `Reservations` và `ReservationTime`
            var dailyBookings = await _context.Reservations
                .Where(r => r.ReservationTime >= startDate)
                .GroupBy(r => r.ReservationTime.Date) // SỬA: Dùng `ReservationTime.Date`
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var values = new List<IList<object>> { new List<object> { "Ngày", "Số lượt đặt" } }; // Hàng tiêu đề
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var booking = dailyBookings.FirstOrDefault(b => b.Date == date);
                values.Add(new List<object> { date.ToString("dd/MM"), booking?.Count ?? 0 });
            }

            // ĐỔI TÊN TAB: để khớp với 4 tab mới
            await WriteToSheetAsync(service, "'DatBanTheoNgay'!A:B", values);
        }

        // [Chart 2] - Thống kê theo Trạng thái (ĐÃ SỬA)
        private async Task UpdateReservationsByStatusAsync(SheetsService service)
        {
            // SỬA: Dùng `Reservations` và `Status` (tên cột Status của bạn đã khớp)
            var stats = await _context.Reservations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var values = new List<IList<object>> { new List<object> { "Trạng thái", "Số lượng" } }; // Hàng tiêu đề
            foreach (var item in stats)
            {
                values.Add(new List<object> { item.Status, item.Count });
            }

            await WriteToSheetAsync(service, "'TrangThaiDatBan'!A:B", values);
        }

        // [Chart 3] - Top 10 Bàn được đặt nhiều nhất (ĐÃ SỬA)
        private async Task UpdateTopTablesAsync(SheetsService service)
        {
            // SỬA: Entity của bạn có quan hệ trực tiếp `Reservation.Table`, rất tốt!
            var stats = await _context.Reservations
                .Include(r => r.Table) // Nối sang bảng Table
                .Where(r => r.Table != null) // Bỏ qua các lượt đặt không có bàn
                .GroupBy(r => r.Table.Name) // Dùng `Table.Name` của bạn
                .Select(g => new { TableName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10) // Lấy top 10
                .ToListAsync();

            var values = new List<IList<object>> { new List<object> { "Tên Bàn", "Số lượt đặt" } }; // Hàng tiêu đề
            foreach (var item in stats)
            {
                values.Add(new List<object> { item.TableName, item.Count });
            }

            await WriteToSheetAsync(service, "'TopBanDatNhieuNhat'!A:B", values);
        }

        // [Chart 4] - Thống kê theo Số người (ĐÃ SỬA)
        private async Task UpdatePartySizeAsync(SheetsService service)
        {
            // SỬA: Dùng `Reservations` và `NumberOfGuests`
            var stats = await _context.Reservations
                .GroupBy(r => r.NumberOfGuests)
                .Select(g => new { PartySize = g.Key, Count = g.Count() })
                .OrderBy(x => x.PartySize)
                .ToListAsync();

            var values = new List<IList<object>> { new List<object> { "Số người", "Số lượt đặt" } }; // Hàng tiêu đề
            foreach (var item in stats)
            {
                values.Add(new List<object> { $"{item.PartySize} người", item.Count });
            }

            await WriteToSheetAsync(service, "'SoNguoiMoiDatBan'!A:B", values);
        }

        // [Chart 5 - MỚI] - Thống kê đặt bàn theo Chi nhánh
        private async Task UpdateReservationsByBranchAsync(SheetsService service)
        {
            // Logic: Reservation -> Table -> Branch
            var stats = await _context.Reservations
                .Include(r => r.Table) // Nối Reservation -> Table
                    .ThenInclude(t => t.Branch) // Nối Table -> Branch
                .Where(r => r.Table != null && r.Table.Branch != null) // Đảm bảo có dữ liệu
                .GroupBy(r => r.Table.Branch.Name) // Nhóm theo tên Chi nhánh
                .Select(g => new { BranchName = g.Key, Count = g.Count() })
                .ToListAsync();

            var values = new List<IList<object>> { new List<object> { "Chi nhánh", "Số lượt đặt" } }; // Hàng tiêu đề
            foreach (var item in stats)
            {
                values.Add(new List<object> { item.BranchName, item.Count });
            }

            await WriteToSheetAsync(service, "'DatBanTheoChiNhanh'!A:B", values);
        }

    }
}
