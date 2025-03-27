using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Logging;

namespace API.Hubs
{
    public class HardwareHub : Hub
    {
        private readonly IHardwareService _hardwareService;

        public HardwareHub(IHardwareService hardwareService)
        {
            _hardwareService = hardwareService;
        }

        public async Task SendHardwareStatus()
        {
            var hardwareInfo = await _hardwareService.GetHardwareInfoAsync();
            await Clients.All.SendAsync("ReceiveHardwareStatus", hardwareInfo);
        }
    }

}
