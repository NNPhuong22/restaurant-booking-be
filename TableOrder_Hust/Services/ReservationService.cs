using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;
using TableOrder_Hust.DTOs;
using TableOrder_Hust.Models;

namespace TableOrder_Hust.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(CreateReservationRequest request);
        Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId);
        Task<IEnumerable<Reservation>> GetReservationsByDateAsync(DateTime date, int branchId);
        Task<Reservation?> ConfirmReservationAsync(int reservationId);
        Task<Reservation?> CancelReservationAsync(int reservationId);
        Task<bool> CheckoutReservationAsync(int reservationId, string? note = null);
        Task<object> GetReservationsByBranchAndDateAsync(int branchId);
    }
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IGoogleCalendarService _calendarService;
        public ReservationService(IGoogleCalendarService calendarService, AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _calendarService = calendarService;
        }

        public async Task<Reservation> CreateReservationAsync(CreateReservationRequest request)
        {
            // kiểm tra bàn có tồn tại không
            var table = await _context.Tables.FindAsync(request.TableId);
            if (table == null)
                throw new Exception("Table not found");

            // kiểm tra bàn có đang trống ở thời điểm đó không
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.TableId == request.TableId
                               && r.ReservationTime == request.ReservationTime
                               && r.Status != "Cancelled");

            if (hasConflict)
                throw new Exception("Table is already reserved at this time");

            var reservation = new Reservation
            {
                UserId = request.UserId,
                CustomerName = request.CustomerName,
                Phone = request.Phone,
                Email = request.Email,
                TableId = request.TableId,
                ReservationTime = request.ReservationTime,
                NumberOfGuests = request.NumberOfGuests,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            await _emailService.SendEmailAsync(
    request.Email,
    "Xác nhận đặt bàn",
    "<h1>Cảm ơn bạn đã đặt bàn!</h1>"
);
            try
            {
                await _calendarService.CreateEventAsync(
                    $"Lịch đặt bàn tại Nhà hàng HUST",
                    $"Chi tiết đặt bàn: Bàn số {request.TableId}, cho {request.NumberOfGuests} người.",
                    request.ReservationTime,
                    request.ReservationTime.AddMinutes(90)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create calendar event: {ex.Message}");
            }

            return reservation;
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Table)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByDateAsync(DateTime date, int branchId)
        {
            return await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.Table.BranchId == branchId
                            && r.ReservationTime.Date == date.Date)
                .ToListAsync();
        }

        public async Task<Reservation?> ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return null;

            reservation.Status = "Confirmed";
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<Reservation?> CancelReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return null;

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return reservation;
        }
        public async Task<bool> CheckoutReservationAsync(int reservationId, string? note = null)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null) return false;

            if (reservation.Status != "Confirmed" && reservation.Status != "CheckedIn")
                return false; // chỉ cho phép checkout nếu đang trong trạng thái hợp lệ

            reservation.Status = "Completed";
            reservation.CheckOutTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<object> GetReservationsByBranchAndDateAsync(int branchId)
        {
            DateTime date = DateTime.Now;
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var reservations = await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.Table.BranchId == branchId
                         && r.ReservationTime >= startOfDay
                         && r.ReservationTime < endOfDay)
                .ToListAsync();

            var confirmed = reservations
                .Where(r => r.Status == "Confirmed")
                .Select(r => new
                {
                    r.Id,
                    r.CustomerName,
                    r.Phone,
                    r.ReservationTime,
                    TableId = r.Table.Id,
                    TableName = r.Table.Name
                })
                .ToList();

            var pending = reservations
                .Where(r => r.Status == "Pending")
                .Select(r => new
                {
                    r.Id,
                    r.CustomerName,
                    r.Phone,
                    r.ReservationTime,
                    TableId = r.Table.Id,
                    TableName = r.Table.Name
                })
                .ToList();

            return new
            {
                Confirmed = confirmed,
                Pending = pending
            };
        }
    }

}
