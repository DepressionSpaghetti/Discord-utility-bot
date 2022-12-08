using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using CliWrap;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Net.Http;
using YoutubeSearchApi.Net.Models.Youtube;
using YoutubeSearchApi.Net.Services;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Dom;

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
        public static List<MemoryStream> songQueue = new List<MemoryStream>();
        public static int queueSize = songQueue.Count - 1;
        public static int playingIndex = 0;
        public static int queueIndex = 0;

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

        public async Task PlayAudio(string name)
        {
            await QueueAudio(name);
            MemoryStream memoryStream = songQueue.ElementAt(playingIndex);

            // Play audio
            do
            {
                using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
                {
                    try { await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                    finally { await discord.FlushAsync(); }
                }

                // Index of current song increases
                playingIndex++;

            }while(playingIndex <= queueSize);

        }

        public async Task QueueAudio(string name) 
        {
            MemoryStream memoryStream = await GetAudioStream(name);
            
            songQueue.Add(memoryStream);



        }

        // Get Youtube metadata stream and pipes it to the bot
        private async Task<MemoryStream> GetAudioStream(string song)
        {
            song = await SearchMusic(song);

            // Get Youtube audio stream
            YoutubeClient youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(song);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            // Pipe it to discord
            MemoryStream memoryStream = new MemoryStream();
            await Cli.Wrap(@"ffmpeg\ffmpeg.exe")
                .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync();

            return memoryStream;
        }

        private async Task<string> SearchMusic(string songName)
        {
            string videoLink = null;

            using (var httpClient = new HttpClient())
            {
                YoutubeSearchClient client = new YoutubeSearchClient(httpClient);
                var responseObject = await client.SearchAsync(songName);
                foreach (YoutubeVideo video in responseObject.Results)
                {
                    videoLink = video.Url;
                }

            }

            return videoLink;
        }

       
    }
}