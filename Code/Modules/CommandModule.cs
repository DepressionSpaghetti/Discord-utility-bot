using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Diagnostics;
using System.Threading.Tasks;


public class CommandModule : ModuleBase<SocketCommandContext>
{

    // connect bot to voice channel command
    [Command("connect", RunMode = RunMode.Async)]
    [Summary("Connects to voice channel where the user is")]
    public async Task JoinChannel(IVoiceChannel channel = null)
    {
        channel ??= (Context.User as IGuildUser)?.VoiceChannel;
        if(channel == null)
        {
            await Context.Channel.SendMessageAsync("User must be in valid voice channel or voice channel must be passed as an argument.");
            return;

        }

        var audioClient = await channel.ConnectAsync();
        
    }


    // disconnect bot from voice channel command
    [Command("disconnect", RunMode = RunMode.Async)]
    [Summary("Disconnects from currently connected voice channel")]
    public async Task LeaveChannel(IVoiceChannel channel = null)
    {

        var botUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
        channel ??= (botUser as IGuildUser)?.VoiceChannel;
        if(channel == null)
        {
            await Context.Channel.SendMessageAsync("Bot must be already connected to voice channel");
            return;

        }


        await channel.DisconnectAsync();
    }

    private Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }

    // play audio command
    [Command("play", RunMode = RunMode.Async)]
    [Summary("Plays audio to connected chanel")]
    private async Task SendAsync(IAudioClient client, string path)
    {
        // Create FFmpeg using the previous example
        using (var ffmpeg = CreateStream(path))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
        {
            try { await output.CopyToAsync(discord); }
            finally { await discord.FlushAsync(); }
        }

        
    }
    
}