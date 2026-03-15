using Microsoft.AspNetCore.SignalR;
using PowerChat.Server.Models;

namespace PowerChat.Server
{
    public class PowerHub : Hub
    {
        // This method is called by clients to send a message to the server,
        // which then broadcasts it to all connected clients.
        public async Task SendPayload(ChatMessage message)
        {
            // Triggers the "ReceiveMessage" event on all connected clients.
            await Clients.Others.SendAsync("ReceiveMessage", message);
        }
    }
}