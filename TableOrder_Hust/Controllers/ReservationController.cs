using Microsoft.AspNetCore.Mvc;
using TableOrder_Hust.DTOs;
using TableOrder_Hust.Services;

namespace TableOrder_Hust.Controllers
{
    [ApiController]
    [Route("api/reservation")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                var reservation = await _reservationService.CreateReservationAsync(request);
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetReservationsByBranchAndDate(int branchId)
        {
            var result = await _reservationService.GetReservationsByBranchAndDateAsync(branchId);
            return Ok(result);
        }

        //Xác nhận đặt bàn
        [HttpPut("confirm/{reservationId}")]
        public async Task<IActionResult> ConfirmReservation(int reservationId)
        {
            var reservation = await _reservationService.ConfirmReservationAsync(reservationId);
            if (reservation == null) return NotFound(new { message = "Reservation not found" });

            return Ok(reservation);
        }

        [HttpPut("cancel/{reservationId}")]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            var reservation = await _reservationService.CancelReservationAsync(reservationId);
            if (reservation == null) return NotFound(new { message = "Reservation not found" });

            return Ok(reservation);
        }

        [HttpPut("{id}/checkout")]
        public async Task<IActionResult> CheckoutReservation(int id, [FromBody] CheckoutRequest request)
        {
            var success = await _reservationService.CheckoutReservationAsync(id, request?.Note);

            if (!success)
                return BadRequest(new { message = "Không thể trả bàn. Vui lòng kiểm tra ID hoặc trạng thái booking." });

            return Ok(new
            {
                message = "Bàn đã được trả thành công",
                reservationId = id,
                status = "Completed",
                checkOutTime = DateTime.UtcNow
            });
        }

    }
}
public class CheckoutRequest
{
    public string? Note { get; set; }
}