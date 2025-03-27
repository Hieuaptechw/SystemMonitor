using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class NetWorkHub : Hub
    {
        private readonly INetworkService _networkService;
        public NetWorkHub(INetworkService networkService)
        {
            _networkService = networkService;
        }
        public async Task SendNetworkStatus()
        {
            var netWorkInfo = await _networkService.GetNetworkInfoAsync();
            await Clients.All.SendAsync("ReceiveNetworkStatus", netWorkInfo);
        }
    }
}
