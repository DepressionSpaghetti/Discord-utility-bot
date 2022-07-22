using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;


public class Program
{
	private DiscordSocketClient _client;
	public static Task Main(string[] args) => new Program().MainAsync();

	public async Task MainAsync()
	{
		_client = new DiscordSocketClient();
		_client.MessageReceived += CommandHandler;
		_client.Log += Log;

		var appConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
		var token = appConfig["DiscordBotToken"];

		await _client.LoginAsync(TokenType.Bot, token);
		await _client.StartAsync();

		// Block this task until the program is closed.
		await Task.Delay(-1);
	}

	private Task Log(LogMessage msg)
	{
		Console.WriteLine(msg.ToString());
		return Task.CompletedTask;
	}

	private Task CommandHandler(SocketMessage message)
    {
		string command = "";
		int lengthOfCommand = -1;


		//command filtering
		if (!message.Content.StartsWith("!")) 
			return Task.CompletedTask;

		if (message.Author.IsBot)
			return Task.CompletedTask;

		if (message.Content.Contains(" "))
			lengthOfCommand = message.Content.IndexOf(" ");
		else
			lengthOfCommand = message.Content.Length;

		command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

		//commands begin here
		if (command.Equals("hello"))
        {
			message.Channel.SendMessageAsync($@"Hello {message.Author.Mention}");

        }


		return Task.CompletedTask;
    }


}