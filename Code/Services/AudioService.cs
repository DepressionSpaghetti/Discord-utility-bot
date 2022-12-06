using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using CliWrap;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;


namespace DiscordBot
{
    public class AudioService
    {
        // Makes ConcurrentDictionary to keep tasks across multiple executions
        public readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        public AudioService()
        {
            ConnectedChannels = new();
        }

        // Task to join audio channel
        public async Task JoinChannel(IGuild guild, IVoiceChannel channel) 
        {
            // Gets channel ID
            if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                return;
            }
            if (channel.Guild.Id != guild.Id)
            {
                return;
            }

            // Connects to voice channel
            await channel.ConnectAsync();

            await Task.Delay(-1);

        }

        // Task to leave channel
        public async Task LeaveChannel(IAudioChannel channel)
        {
            await channel.DisconnectAsync();

        }

        // Task to play music
        /*public async Task SendAudio(IGuild guild, IMessageChannel channel, string path)
        {
            // Bot sends message that file doesnt exist
            if(!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist");
                return;

            }

            // Plays given file
            if (ConnectedChannels.TryGetValue(guild.Id, out IAudioClient client))
            {
                using (var ffmpeg = CreateProcess(path))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                    finally { await stream.FlushAsync(); }

                }

            }
        }*/

        // Get Youtube metadata stream
        private async Task CreateProcess()
        {
            YoutubeClient youtube = new YoutubeClient();
            var StreamManifest = await youtube.Videos.Streams.GetManifestAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            var StreamInfo = await StreamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = youtube.Videos.Streams.GetAsync(StreamInfo);

        }


       
    }
}