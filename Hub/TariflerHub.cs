using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MyGoldenFood.Hubs
{
    public class TariflerHub : Hub
    {
        public async Task NotifyTarifUpdated()
        {
            await Clients.All.SendAsync("TarifUpdated");
        }
    }
}
