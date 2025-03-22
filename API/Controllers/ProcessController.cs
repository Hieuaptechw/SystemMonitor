using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Domain.Entity;

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

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessInfo>>> GetRunningProcesses()
        {
            var processes = await _processService.GetRunningApplicationsAsync();
            return Ok(processes);
        }

    }
}
