using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableOrder_Hust.Data;

namespace TableOrder_Hust.Controllers
{
    [Route("api/branches")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BranchesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _context.Branches
                .Select(b => new { b.Id, b.Name, b.Address, b.Latitude, b.Longitude })
                .ToListAsync();
            return Ok(branches);
        }
    }
}
