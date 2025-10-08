using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;
using TableOrder_Hust.Models;
using TableOrder_Hust.Services;

namespace TableOrder_Hust.Controllers
{
    [ApiController]
    [Route("api/tables")]
    public class TablesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITableService _tableService;
        public TablesController(AppDbContext db, ITableService tableService) { _db = db; _tableService = tableService; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Tables.ToListAsync());

        [HttpGet("table-by-branch/{branchId}")]
        public async Task<IActionResult> GetByBranch(int branchId)
        {
            var tables = await _db.Tables
                .Where(t => t.BranchId == branchId)
                .ToListAsync();

            if (tables == null || !tables.Any())
                return NotFound($"Không tìm thấy bàn nào trong chi nhánh có id = {branchId}");

            return Ok(tables);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Table table)
        {
            _db.Tables.Add(table);
            await _db.SaveChangesAsync();
            return Ok(table);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Table table)
        {
            if (id != table.Id) return BadRequest();
            _db.Entry(table).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Tables.FindAsync(id);
            if (t == null) return NotFound();
            _db.Tables.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableTables(
       [FromQuery] int branchId,
       [FromQuery] DateTime dateTime,
       [FromQuery] int numberOfGuests)
        {
            var tables = await _tableService.GetAvailableTablesAsync(branchId, dateTime, numberOfGuests);
            return Ok(tables);
        }
    }
}
