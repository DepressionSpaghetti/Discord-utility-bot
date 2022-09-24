using Discord;
using Discord.Commands;
using System.Threading.Tasks;

public class CommandModule : ModuleBase<SocketCommandContext>
{

    //uses the AudioService class
    private readonly AudioService _service;

    //ads AudioService instance
    public CommandModule(AudioService service)
    {
        _service = service;
    }


    // connect bot to voice channel command
    [Command("connect", RunMode = RunMode.Async)]
    [Summary("Connects to voice channel where the user is")]
    public async Task JoinChnl()
    {
        await _service.JoinChannel(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

    }


    // disconnect bot from voice channel command
    [Command("disconnect", RunMode = RunMode.Async)]
    [Summary("Disconnects from currently connected voice channel")]
    public async Task LeaveChnl()
    {
        await _service.LeaveChannel(Context.Guild);

    }
    

    // play audio command
    [Command("play", RunMode = RunMode.Async)]
    [Summary("Plays audio to connected chanel")]
    public async Task PlayAudio([Remainder] string song)
    {
        await _service.SendAudio(Context.Guild, Context.Channel, song);

    }
    
}