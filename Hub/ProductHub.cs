using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MyGoldenFood.Hubs
{
    public class ProductHub : Hub
    {
        public async Task NotifyProductUpdated()
        {
            await Clients.All.SendAsync("ProductUpdated");
        }
    }
}
