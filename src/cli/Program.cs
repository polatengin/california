using System.CommandLine;
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

await rootCommand.InvokeAsync(args);
