using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

//general log messages in prompt
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

    #region private instances
    private IServiceProvider _services;
    private DiscordSocketClient _client;
    private CommandService _commands;
    private CommandHandler _commandHandler;
    #endregion

    //creates service provider instance
    public Program()
    {
        _services = CreateProvider();

    }

    static void Main(string[] args)
        => new Program().RunAsync(args).GetAwaiter().GetResult();

    //creates dependency injection
    static IServiceProvider CreateProvider()
    {
        //ads service collection
        var collection = new ServiceCollection()
            .AddSingleton<AudioService>();

        //builds the service provider for dependency injection
        return collection.BuildServiceProvider();
    }

    async Task RunAsync(string[] args)
    {
        var client = _services.GetRequiredService<AudioService>();
        _client = new DiscordSocketClient();
         _commands = new CommandService();
         _commandHandler = new CommandHandler(_client, _commands);
         _client.Log += Log;
         _commands.Log += Log;

        //gets bot token from JSON file in config
         var appConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
         var token = appConfig["DiscordBotToken"];

        //starts bot
         await _client.LoginAsync(TokenType.Bot, token);
         await _client.StartAsync();
         await _commandHandler.InstallCommandsAsync();       //awaits commands

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

        //adds commands from files in Modules folder
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
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

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

     //writes log messages to prompt
     private Task Log(LogMessage msg)
     {
         Console.WriteLine(msg.ToString());
         return Task.CompletedTask;

     }

 }