using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {

        // Injects the audio service
        private readonly AudioService _service;
    
        public CommandModule(AudioService service)
        {
            _service = service;
        }


        // Connects bot to voice channel
        [Command("connect", RunMode = RunMode.Async)]
        [Summary("Connects to voice channel where the user is")]
        public async Task JoinChnl(IVoiceChannel chnl = null)
        {
            if (chnl == null) await _service.JoinChannel(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            else await _service.JoinChannel(Context.Guild, chnl);

        }


        // Disconnect bot from voice channel
        [Command("disconnect", RunMode = RunMode.Async)]
        [Summary("Disconnects from currently connected voice channel")]
        public async Task LeaveChnl()
        {
            // Gets voice channel
            var VoiceChnl = Context.Guild.CurrentUser.VoiceChannel;

            // If not connected to any voice channel lets the user know
            if (VoiceChnl == null)
            {
                await Context.Channel.SendMessageAsync("Not connected to any voice channels, can't disconnect.");

            }

            // Leaves channel
            else await _service.LeaveChannel(VoiceChnl);

        }
    
    /*
        // play audio command
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Plays audio to connected chanel")]
        public async Task PlayAudio([Remainder] string song)
        {
            await JoinChnl();

            await _service.SendAudio(Context.Guild, Context.Channel, song);

        }
    */
    }
}