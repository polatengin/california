using Microsoft.AspNetCore.SignalR;

namespace Bingo.Server.Hubs;

public enum GameStatus
{
  Waiting,
  Started,
  Finished
}

public record BingoGame
{
  public string Name { get; set; }
  public GameStatus Status { get; set; }
  public HashSet<Tuple<string, string>> Players { get; set; }
}

public class BingoHub : Hub
{
  public static Dictionary<string, BingoGame> _clients = new();

  public async Task<bool> JoinGroup(string groupName, string playerName)
  {
    var group = _clients.GetValueOrDefault(groupName);
    if (group == null)
    {
      return false;
    }

    group.Players.Add(Tuple.Create(Context.ConnectionId, playerName));

    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    await Clients.Group(groupName).SendAsync("UpdateGame", group);

    return true;
  }

  public async Task<bool> CreateGroup(string groupName, string playerName)
  {
    var group = _clients.GetValueOrDefault(groupName);
    if (group != null)
    {
      return false;
    }

    group = new()
    {
      Name = groupName,
      Status = GameStatus.Waiting,
      Players = new()
    };
    group.Players.Add(Tuple.Create(Context.ConnectionId, playerName));

    _clients.Add(groupName, group);

    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    await Clients.Group(groupName).SendAsync("UpdateGame", group);

    return true;
  }

  public async Task<bool> StartGame(string groupName)
  {
    var group = _clients.GetValueOrDefault(groupName);
    if (group != null)
    {
      return false;
    }

    await Clients.Group(groupName).SendAsync("UpdateGame", group);

    return true;
  }

  public override Task OnDisconnectedAsync(Exception? exception)
  {
    foreach (var group in _clients)
    {
      foreach (var connection in group.Value.Players)
      {
        if (connection.Item1 == Context.ConnectionId)
        {
          var set = group.Value.Players;
          set.Remove(connection);

          Clients.Group(group.Key).SendAsync("UpdateGame", group).Wait();

          if (set.Count == 0)
          {
            Console.WriteLine($"Group {group.Key} has been removed");
            _clients.Remove(group.Key);
          }
        }
      }
    }

    Console.WriteLine($"{Context.ConnectionId} has disconnected");

    return base.OnDisconnectedAsync(exception);
  }
}
