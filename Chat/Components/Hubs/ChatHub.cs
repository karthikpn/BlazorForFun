namespace Chat.Components.Hubs;

using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        var username = Context.User?.Identity?.Name;

        await Clients.All.SendAsync(
            "ReceiveMessage",
            username,
            message
        );
    }
}
