using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class ProcessHub : Hub
    {
        private readonly IProcessService _processService;
        public ProcessHub(IProcessService processService )
        {
            _processService = processService;
        }
        public async Task SendProcessStatus()
        {
            var processInfo = await _processService.GetRunningApplicationsAsync();
            await Clients.All.SendAsync("ReceiveProcessStatus", processInfo);
        }
    }
}
