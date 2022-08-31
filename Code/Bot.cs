using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;

public class LoggingService
 {
     public LoggingService(DiscordSocketClient client, CommandService command)
     {
         client.Log += LogAsync;
         command.Log += LogAsync;
     }
     private Task LogAsync(LogMessage message)
     {
         if (message.Exception is CommandException cmdException)
         {
             Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                 + $" failed to execute in {cmdException.Context.Channel}.");
             Console.WriteLine(cmdException);
         }
         else
             Console.WriteLine($"[General/{message.Severity}] {message}");

         return Task.CompletedTask;
     }
 }

 public class Program
 {
     private DiscordSocketClient _client;
     private CommandService _commands;
     private CommandHandler commandHandler;

     public static Task Main(string[] args) => new Program().MainAsync();

     public async Task MainAsync()
     {
         _client = new DiscordSocketClient();
         _commands = new CommandService();
         commandHandler = new CommandHandler(_client, _commands);
         //_client.MessageReceived += CommandHandler;
         _client.Log += Log;
         _commands.Log += Log;

         var appConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
         var token = appConfig["DiscordBotToken"];

         await _client.LoginAsync(TokenType.Bot, token);
         await _client.StartAsync();
         await commandHandler.InstallCommandsAsync();

         // Block this task until the program is closed.
         await Task.Delay(-1);
     }

     public class CommandHandler
     {
         private readonly DiscordSocketClient _client;
         private readonly CommandService _commands;

         // Retrieve client and CommandService instance via ctor
         public CommandHandler(DiscordSocketClient client, CommandService commands)
         {
             _commands = commands;
             _client = client;
         }

         public async Task InstallCommandsAsync()
         {
             // Hook the MessageReceived event into our command handler
             _client.MessageReceived += HandleCommandAsync;

             // Here we discover all of the command modules in the entry 
             // assembly and load them. Starting from Discord.NET 2.0, a
             // service provider is required to be passed into the
             // module registration method to inject the 
             // required dependencies.
             //
             // If you do not use Dependency Injection, pass null.
             // See Dependency Injection guide for more information.
             await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                             services: null);
         }

         private async Task HandleCommandAsync(SocketMessage messageParam)
         {
             // Don't process the command if it was a system message
             if (messageParam is not SocketUserMessage message) return;

             // Create a number to track where the prefix ends and the command begins
             int argPos = 0;

             // Determine if the message is a command based on the prefix and make sure no bots trigger commands
             if (!(message.HasCharPrefix('!', ref argPos) ||
                 message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                 message.Author.IsBot)
                 return;

             // Create a WebSocket-based command context based on the message
             var context = new SocketCommandContext(_client, message);

             // Execute the command with the command context we just
             // created, along with the service provider for precondition checks.
             await _commands.ExecuteAsync(
                 context: context,
                 argPos: argPos,
                 services: null);

         }

     }

     private Task Log(LogMessage msg)
     {
         Console.WriteLine(msg.ToString());
         return Task.CompletedTask;
     }



 }