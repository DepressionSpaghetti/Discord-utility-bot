using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace DiscordBot
{
    public class AudioService
    {
        // Makes ConcurrentDictionary to keep tasks across multiple executions
        public readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        public List<CommandModule> Things { get; }
        public AudioService()
        {
            Things = new();
        }

        // Task to join audio channel
        public async Task JoinChannel(IGuild guild, IVoiceChannel channel) 
        {
            // Gets channel ID
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (channel.Guild.Id != guild.Id)
            {
                return;
            }

            // Connects to voice channel
            var audioClient = await channel.ConnectAsync();

        }

        // Task to leave channel
        public async Task LeaveChannel(IAudioChannel channel)
        {
            await channel.DisconnectAsync();

        }

        // Task to play music
        public async Task SendAudio(IGuild guild, IMessageChannel channel, string path)
        {
            // Bot sends message that file doesnt exist
            if(!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist");
                return;

            }

            // Plays given file
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

        // Creates ffmpeg process
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
}