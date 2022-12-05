using Microsoft.AspNetCore.SignalR;

namespace Bingo.Server.Hubs;

public class BingoHub : Hub
{
  public async Task JoinGroup(string groupName)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    await Clients.Group(groupName).SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined the group {groupName}.");

    Console.WriteLine($"{Context.ConnectionId} has joined the group {groupName}.");
  }
}
