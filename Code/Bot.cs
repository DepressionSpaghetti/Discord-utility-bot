using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
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
        static void Main(string[] args)
            => new Program().RunAsync(args).GetAwaiter().GetResult();
        
        #region instances
        private static CommandService _commands;
        private static IServiceProvider _service;
        private CommandHandler _commandHandler;
        #endregion

        public class CommandHandler
        {
            private readonly CommandService _commands;
            private readonly IServiceProvider _services;
            private readonly DiscordSocketClient _client;

            // Retrieve client, CommandService and IServiceProvider
            public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
            {
                _client = client;
                _commands = commands;
                _services = services;
            }

            // Discovers and adds commands
            public async Task InstallCommandsAsync()
            {
                // Hooks the MessageReceived event into the command handler
                _client.MessageReceived += HandleCommandAsync;

                // Discovers Modules and adds them
                await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                                services: _services);
            }

            // Checks messages for commands
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

                // Execute the command
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: _services);
            }

        }

        // Creates dependency collection
        public Program()
        {
            _service = CreateProvider();
        }

        // Creates dependency injection
        static IServiceProvider CreateProvider()
        {
            // Configures CommandService
            var CommandServiceConfig = new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
                LogLevel = LogSeverity.Debug,
                SeparatorChar = ','
            };
            // Configures DiscordSocket
            var SocketConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                LogGatewayIntentWarnings = true
            };

            // Adds singletons to collection
            var collection = new ServiceCollection()
                .AddSingleton(SocketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(CommandServiceConfig)
                .AddSingleton<CommandService>()
                .AddSingleton<AudioService>();

            // Builds the service provider
            return collection.BuildServiceProvider();
        }

        // Main program
        async Task RunAsync(string[] args)
        {

            // Initializes classes and installs commands
            var _client = _service.GetRequiredService<DiscordSocketClient>();
            _commands = new CommandService();
            _commandHandler = new CommandHandler(_client, _commands, _service);
            await _commandHandler.InstallCommandsAsync();

            // Adds to log
            _client.Log += Log;
            _commands.Log += Log;


            // Gets bot token from JSON file
            var appConfig = new ConfigurationBuilder()
                .AddJsonFile($@"config\token.json")
                .Build();
            var token = appConfig["DiscordBotToken"];


            // Starts bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed
            await Task.Delay(-1);
        }

        // Writes log messages to prompt
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;

        }

    }
}