using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;
using TableOrder_Hust.Models;

namespace TableOrder_Hust.Services
{
    public interface ITableService
    {
        Task<Table> CreateTable(Table table);
        Task<Table?> GetTableById(int id);
        Task<IEnumerable<Table>> GetAllTables();
        Task<Table> UpdateTable(Table table);
        Task<bool> DeleteTable(int id);
        Task<IEnumerable<Table>> GetAvailableTablesAsync(int branchId, DateTime dateTime, int numberOfGuests);

    }
    public class TableService : ITableService
    {
        private readonly AppDbContext _context;

        public TableService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Table> CreateTable(Table table)
        {
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<Table?> GetTableById(int id)
        {
            return await _context.Tables.FindAsync(id);
        }

        public async Task<IEnumerable<Table>> GetAllTables()
        {
            return await _context.Tables.ToListAsync();
        }

        public async Task<Table> UpdateTable(Table table)
        {
            _context.Tables.Update(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task<bool> DeleteTable(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null) return false;

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Table>> GetAvailableTablesAsync(int branchId, DateTime dateTime, int numberOfGuests)
        {
            var reservedTableIds = await _context.Reservations
                .Where(r => r.ReservationTime == dateTime && r.Status != "Cancelled")
                .Select(r => r.TableId)
                .ToListAsync();

            var availableTables = await _context.Tables
                .Where(t => t.BranchId == branchId
                            && t.Capacity >= numberOfGuests
                            && !reservedTableIds.Contains(t.Id))
                .ToListAsync();

            return availableTables;
        }

    }
}
