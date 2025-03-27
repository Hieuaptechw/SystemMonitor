using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/protected")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
       
        [Authorize]
        [HttpGet("secure-data")]  
        public IActionResult GetSecureData()
        {
            return Ok(new { message = "Bạn đã được xác thực!" });
        }

    
    }
}
