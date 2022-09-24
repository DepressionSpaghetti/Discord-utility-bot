using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

public class AudioService
{
    private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

    //task to join audio channel
    public async Task JoinChannel(IGuild guild, IVoiceChannel channel)
    {
        //gets coice channel id
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            return;
        }
        if (channel.Guild.Id != guild.Id)
        {
            return;
        }

        //connects to voice channel
        var audioClient = await channel.ConnectAsync();

    }

    //task to leave channel
    public async Task LeaveChannel(IGuild guild)
    {
        IAudioClient client;
        if (ConnectedChannels.TryRemove(guild.Id, out client))
        {
            await client.StopAsync();
            
        }
    }

    //task to play music
    public async Task SendAudio(IGuild guild, IMessageChannel channel, string path)
    {
        //bot sends message that file doesnt exist
        if(!File.Exists(path))
        {
            await channel.SendMessageAsync("File does not exist");
            return;

        }

        //plays given file
        IAudioClient client;
        if(ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            using (var ffmpeg = CreateProcess(path))
            using (var stream = client.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                finally { await stream.FlushAsync(); }

            }

        }
    }

    //creates ffmpeg process
    private Process CreateProcess(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s161e -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}