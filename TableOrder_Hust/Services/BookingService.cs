using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;
using TableOrder_Hust.Models;

namespace TableOrder_Hust.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBooking(Booking booking);
        Task<Booking?> GetBookingById(int id);
        Task<IEnumerable<Booking>> GetBookingsByUser(int userId);
        Task<IEnumerable<Booking>> GetAllBookings();
        Task<bool> CancelBooking(int id);
    }

    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateBooking(Booking booking)
        {
            // Kiểm tra xem bàn có sẵn không
            var isAvailable = !_context.Bookings.Any(b =>
                b.TableId == booking.TableId &&
                b.Date == booking.Date &&
                b.Status == "Confirmed");

            if (!isAvailable)
                throw new Exception("Table is already booked at this time.");

            booking.Status = "Confirmed";
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetBookingById(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Table)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUser(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Table)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllBookings()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Table)
                .ToListAsync();
        }

        public async Task<bool> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
