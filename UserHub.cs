using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RTMAuthServer.Hubs
{
    public class UserHub : Hub
    {
        public async Task NotifyChange(string message)
        {
            await Clients.All.SendAsync("UserUpdate", message);
        }
    }
}
