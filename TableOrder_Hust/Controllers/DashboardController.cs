using Microsoft.AspNetCore.Mvc;
using TableOrder_Hust.Services;

namespace TableOrder_Hust.Controllers
{

    /*
     * Đặt bàn theo ngày
     * <iframe width="600" height="371" seamless frameborder="0" scrolling="no" src="https://docs.google.com/spreadsheets/d/e/2PACX-1vSwxc27vumN76JBIyEaY5P1_B20tjJFtBkO_SI4-FfGvAeZwjXr92d-zlj_UrIcU9SF6wzQljc9XHHZ/pubchart?oid=2054268766&amp;format=interactive"></iframe>
     * Trạng thái đặt bàn
     * <iframe width="600" height="371" seamless frameborder="0" scrolling="no" src="https://docs.google.com/spreadsheets/d/e/2PACX-1vSwxc27vumN76JBIyEaY5P1_B20tjJFtBkO_SI4-FfGvAeZwjXr92d-zlj_UrIcU9SF6wzQljc9XHHZ/pubchart?oid=1837851338&amp;format=interactive"></iframe>
     * Top đặt bàn nhiều nhất
     * <iframe width="600" height="371" seamless frameborder="0" scrolling="no" src="https://docs.google.com/spreadsheets/d/e/2PACX-1vSwxc27vumN76JBIyEaY5P1_B20tjJFtBkO_SI4-FfGvAeZwjXr92d-zlj_UrIcU9SF6wzQljc9XHHZ/pubchart?oid=351339983&amp;format=interactive"></iframe>
     * Số người mỗi lượt đặt
     * <iframe width="600" height="371" seamless frameborder="0" scrolling="no" src="https://docs.google.com/spreadsheets/d/e/2PACX-1vSwxc27vumN76JBIyEaY5P1_B20tjJFtBkO_SI4-FfGvAeZwjXr92d-zlj_UrIcU9SF6wzQljc9XHHZ/pubchart?oid=383879972&amp;format=interactive"></iframe>
     * Đặt bàn theo chi nhánh
     * <iframe width="600" height="371" seamless frameborder="0" scrolling="no" src="https://docs.google.com/spreadsheets/d/e/2PACX-1vSwxc27vumN76JBIyEaY5P1_B20tjJFtBkO_SI4-FfGvAeZwjXr92d-zlj_UrIcU9SF6wzQljc9XHHZ/pubchart?oid=1583422188&amp;format=interactive"></iframe>
     * 

     
     */

    [ApiController]
    [Route("api/users")]
    public class DashboardController : ControllerBase
    {
        private readonly IGoogleSheetsService _googleService;

        public DashboardController(IGoogleSheetsService googleService)
        {
            _googleService = googleService;
        }

        [HttpPost("refresh-sheet-data")]
        public async Task<IActionResult> RefreshSheetData()
        {
            try
            {
                await _googleService.UpdateAllDashboardsAsync();
                return Ok("Đã làm mới dữ liệu 5 bảng trên Google Sheet.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

    }
}
