using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Restaurant.Api.Hubs;

[Authorize]
public sealed class StaffHub : Hub;

[AllowAnonymous]
public sealed class PublicHub : Hub
{
    public Task JoinReservation(string code) => Groups.AddToGroupAsync(Context.ConnectionId, $"reservation:{code}");
    public Task JoinMenu() => Groups.AddToGroupAsync(Context.ConnectionId, "public-menu");
    public Task JoinTable(string code, string sig) => Task.CompletedTask;
}
