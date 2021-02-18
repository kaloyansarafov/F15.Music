using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using F15.Services;
using Discord;
using Discord.WebSocket;

namespace F15.Music.Modules
{
    public class DMModule : ModuleBase
    {
        [Command("switch")]
        private async Task switchChannel(int index = -1)
        {
            if (index >= 0 && index <= CommandHandler.activeDMChannels.Count)
            {
                CommandHandler.activeDMChannel = CommandHandler.activeDMChannels[index];
                await ReplyAsync($"Switched DM channel to {CommandHandler.activeDMChannel.Recipient.Username}'s channel");
            }
            else
            {
                await ReplyAsync("Please enter a number corresponding to the DM channels below:");
                await GetActiveDm();
            }
        }
        [Command("get dms")]
        private async Task GetActiveDm()
        {
            string message = "";
            int count = 0;
            foreach (SocketDMChannel dmChannel in CommandHandler.activeDMChannels)
            {
                message = message + $"\n **{count}**: {dmChannel.Recipient.Username}";
                count++;
            }
            if (message == "") { message = "No active DM channels"; }
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Title = "Active dm channels",
                Description = message,
                Color = new Color(0, 170, 230),
                ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
            }.Build());

        }
        [Command("close")]
        private async Task CloseDM(int index = -1)
        {
            if (index != -1 && index <= CommandHandler.activeDMChannels.Count && index >= 0)
            {
                await CommandHandler.activeDMChannels[index].CloseAsync();
                CommandHandler.activeDMChannels.RemoveAt(index);
                CommandHandler.activeDMChannel = null;
                await ReplyAsync("DM Channel closed");
            }
            else
            {
                await ReplyAsync("Please enter a number corresponding to the DM channels below:");
                await GetActiveDm();
            }
        }
    }
}
