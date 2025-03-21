using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HardwareController : ControllerBase
    {
        private readonly IHardwareService _hardwareService;
        public HardwareController(IHardwareService hardwareService)
        {
            _hardwareService = hardwareService;
        }
        [HttpGet]
        public async Task<IActionResult> GetHardwareInfo()
        {
            var hardwareInfo = await _hardwareService.GetHardwareInfoAsync();
            if (hardwareInfo == null)
            {
                return NotFound(new { message = "Không thể lấy thông tin phần cứng" });
            }
            return Ok(hardwareInfo);
        }
    }
}
