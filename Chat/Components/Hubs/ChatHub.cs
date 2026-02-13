namespace Chat.Components.Hubs;

using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string username, string message)
    {
        await Clients.All.SendAsync(
            "ReceiveMessage",
            username,
            message
        );
    }
}
