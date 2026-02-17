using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Comanda.Api.Hubs;

public class OrdersHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.GetHttpContext()?.Request.Query["role"].ToString();
        if (string.Equals(role, "Cook", StringComparison.OrdinalIgnoreCase))
            await Groups.AddToGroupAsync(Context.ConnectionId, "cooks");
        else if (string.Equals(role, "Waiter", StringComparison.OrdinalIgnoreCase))
            await Groups.AddToGroupAsync(Context.ConnectionId, "waiters");

        await base.OnConnectedAsync();
    }
}
