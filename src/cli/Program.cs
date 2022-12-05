using System.CommandLine;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR.Client;

var hostOption = new Option<bool>(name: "--host", description: "To host a bingo game");
var joinOption = new Option<string>(name: "--join", description: "To host a bingo game");

var rootCommand = new RootCommand();
var sub1Command = new Command("host", "First-level subcommand");
rootCommand.Add(sub1Command);

sub1Command.SetHandler(() =>
{
  Console.WriteLine("host");
});

var sub2Command = new Command("join", "Second level subcommand");
rootCommand.Add(sub2Command);

sub2Command.SetHandler(() =>
{
  Console.WriteLine("join");
});

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5186/hub")
    .Build();

connection.Closed += async (error) =>
{
  Console.WriteLine("Connection closed");
  await Task.Delay(new Random().Next(10, 15) * 1000);
  Console.WriteLine("retrying...");
  await connection.StartAsync();
  Console.WriteLine("Connected to server");
};

connection.On<string, string>("JoinGroup", (user, message) =>
{
  Console.WriteLine($"{user}: {message}");
});

await connection.StartAsync();

Console.WriteLine("Connected to server");

await rootCommand.InvokeAsync(args);
var tcs = new TaskCompletionSource();
using var reg = PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => tcs.TrySetResult());
await tcs.Task;

Console.WriteLine("Closing");
