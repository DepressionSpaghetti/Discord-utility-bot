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
using Discord.Audio.Streams;
using System.Runtime.Remoting.Contexts;

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

        public static IAudioClient client;

        // Task to join audio channel
        public async Task JoinChannel(IGuild guild, IVoiceChannel channel) 
        {
            // Gets channel ID
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (channel.Guild.Id != guild.Id)
            {
                return;
            }

            // Connects to voice channel
            client = await channel.ConnectAsync();

        }

        // Task to leave channel
        public async Task LeaveChannel(IAudioChannel channel)
        {
            await channel.DisconnectAsync();

        }

        // Get Youtube metadata stream and pipes it to the bot
        public async Task SendAudio(string videoLink)
        {


            // Get Youtube audio stream
            YoutubeClient youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            // Pipe it to discord
            MemoryStream memoryStream = new MemoryStream();
            await Cli.Wrap(@"ffmpeg\ffmpeg.exe")
                .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync();

            // Play it
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                finally { await discord.FlushAsync(); }
            }


        }


       
    }
}