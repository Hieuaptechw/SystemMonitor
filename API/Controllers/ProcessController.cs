using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessController : ControllerBase
    {
        private readonly IProcessService _processService;

     
        public ProcessController(IProcessService processService)
        {
            _processService = processService;
        }

        [Authorize]
        [HttpGet]
  
        public async Task<ActionResult<IEnumerable<ProcessInfo>>> GetRunningProcesses()
        {
            var processes = await _processService.GetRunningApplicationsAsync();
            return Ok(processes);
        }

    }
}
