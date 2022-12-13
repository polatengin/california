using System.CommandLine;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR.Client;

var _gameName = string.Empty;
var _game = new BingoGame();

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hub")
    .WithAutomaticReconnect()
    .Build();

async Task ConnectAsync()
{
  try
  {
    await connection.StartAsync();
  }
  catch (Exception ex)
  {
    Console.WriteLine(ex.Message);

    Environment.Exit(1);
  }
}

async Task RenderMenuAsync()
{
  Console.Clear();

  Console.WriteLine("List of people connected to the game:");

  foreach (var client in _game.Players)
  {
    Console.WriteLine(client.Item2);
  }

  Console.WriteLine("Press any key to start the game");

  Console.ReadLine();

  await connection.InvokeAsync("StartGame", _gameName);
}

connection.Closed += async (error) =>
{
  Console.WriteLine(await Task.FromResult("Connection closed"));
};

connection.On<BingoGame>("UpdateGame", async (game) =>
{
  _game = game;

  await RenderMenuAsync();
});

var rootCommand = new RootCommand();

var playerNameOption = new Option<string>("--player-name", "The name of the player");
playerNameOption.AddAlias("-p");

var gameNameOption = new Option<string>("--game-name", "The name of the game");
gameNameOption.AddAlias("-g");

var hostCommand = new Command("host", "Host a new bingo game with your friends");
hostCommand.AddOption(gameNameOption);
hostCommand.AddOption(playerNameOption);
rootCommand.Add(hostCommand);

hostCommand.SetHandler(async (gameName, playerName) =>
{
  if (string.IsNullOrWhiteSpace(playerName))
  {
    playerName = $"Anonymous {Random.Shared.Next(0, 1000)}";
  }

  if (string.IsNullOrWhiteSpace(gameName))
  {
    gameName = Guid.NewGuid().ToString();
  }

  _gameName = gameName;

  await ConnectAsync();

  Console.WriteLine("Connected to server");

  var result = await connection.InvokeAsync<bool>("CreateGroup", gameName, playerName);

  if (result)
  {
    await RenderMenuAsync();
  }
  else
  {
    Console.WriteLine($"Game {gameName} already exists");
  }

}, gameNameOption, playerNameOption);

var joinCommand = new Command("join", "Join a bingo game");
joinCommand.AddOption(gameNameOption);
joinCommand.AddOption(playerNameOption);
rootCommand.Add(joinCommand);

joinCommand.SetHandler(async (gameName, playerName) =>
{
  if (string.IsNullOrWhiteSpace(playerName))
  {
    playerName = $"Anonymous {Random.Shared.Next(0, 1000)}";
  }

  if (string.IsNullOrWhiteSpace(gameName))
  {
    Console.WriteLine("Please provide a game name");
    return;
  }

  _gameName = gameName;

  await ConnectAsync();

  var result = await connection.InvokeAsync<bool>("JoinGroup", gameName, playerName);

  if (result)
  {
    await RenderMenuAsync();
  }
  else
  {
    Console.WriteLine($"Game {gameName} does not exist");
  }

}, gameNameOption, playerNameOption);

await rootCommand.InvokeAsync(args);
var tcs = new TaskCompletionSource();
using var reg = PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => tcs.TrySetResult());
await tcs.Task;

Console.WriteLine("Closing");

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
