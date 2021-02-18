using Discord;
using Discord.Commands;
using Discord.WebSocket;
using F15.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace F15.Music.Modules
{
    namespace F15.Modules
    {
        public class AudioModule : ModuleBase<SocketCommandContext>
        {
            public LavaLinkAudio AudioService { get; set; }


            [Command("Join")]
            public async Task JoinAndPlay()
                => await ReplyAsync(embed: await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

            [Command("Leave")]
            public async Task Leave()
                => await ReplyAsync(embed: await AudioService.LeaveAsync(Context.Guild));

            [Command("Play")]
            [Alias("P")]
            public async Task Play([Remainder] string search)
                => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));

            [Command("Stop")]
            public async Task Stop()
                => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild));

            [Command("List")]
            [Alias("Q","Queue")]
            public async Task List()
                => await ReplyAsync(embed: await AudioService.ListAsync(Context.Guild));

            [Command("Skip")]
            [Alias("S")]
            public async Task Skip()
                => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild));

            [Command("Volume")]
            public async Task Volume(int volume)
                => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume));

            [Command("Pause")]
            public async Task Pause()
                => await ReplyAsync(await AudioService.PauseAsync(Context.Guild));

            [Command("Resume")]
            public async Task Resume()
                => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild));
        }
    }

}
