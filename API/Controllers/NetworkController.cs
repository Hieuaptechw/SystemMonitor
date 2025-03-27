using Application.Interfaces;
using Domain.Entity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkController : ControllerBase
    {
        private readonly INetworkService _networkService;
        public NetworkController(INetworkService networkService)
        {
            _networkService = networkService;
        }
        [Authorize]
        [HttpGet]
 
        public async Task<ActionResult<IEnumerable<NetworkInfo>>> GetNetworkInfo()
        {
            var network = await _networkService.GetNetworkInfoAsync();
            return Ok(network);
        }

    }
}
