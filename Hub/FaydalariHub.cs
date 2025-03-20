using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MyGoldenFood.Hubs
{
    public class FaydalariHub : Hub
    {
        public async Task NotifyBenefitUpdated()
        {
            await Clients.All.SendAsync("BenefitUpdated");
        }
    }
}
