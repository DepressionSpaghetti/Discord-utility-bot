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

}