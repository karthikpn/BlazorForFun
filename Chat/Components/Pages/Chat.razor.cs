using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Components.Pages;

public partial class Chat : ComponentBase, IAsyncDisposable
{
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    HubConnection? hub;
    string user = "Karthik";
    string message = "";
    List<string> messages = new();

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        var user = principal.Identity?.Name;

        hub = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("chatHub"))
            .Build();

        hub.On<string, string>("ReceiveMessage", (u, m) =>
        {
            messages.Add($"{u}: {m}");
            InvokeAsync(StateHasChanged);
        });

        await hub.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (hub is not null)
        {
            await hub.StopAsync();
            await hub.DisposeAsync();
        }
    }

    protected async Task Send()
    {
        if (hub is not null)
        {
            await hub.SendAsync("SendMessage", user, message);
            message = "";
        }
    }
}