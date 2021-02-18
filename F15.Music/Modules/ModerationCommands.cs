using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using F15;
using F15.Services;

namespace F15.Music.Modules
{
    public class ModerationCommands : ModuleBase
    {
        [Command("ban"), RequireBotPermission(GuildPermission.BanMembers), RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(SocketGuildUser user = null, [Remainder] string reason = "")
        {
            SocketGuild gld = Context.Guild as SocketGuild;
            SocketRole mods = gld.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
            SocketRole owner = gld.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;
            SocketGuildUser Author = Context.User as SocketGuildUser;
            if (Author.Roles.Contains(mods) || Author.Roles.Contains(owner) || !(Context.Channel.Id == 326078054948667393))
            {
                if (gld.Users.Contains(user))
                {
                    if (user.Id == 348505747828506624) { await ReplyAsync("How about, NO!"); }
                    if (user == null) { await ReplyAsync("Please specify a user to ban!"); return; }
                    else
                    {
                        await ReplyAsync($"{user.Mention} has been banned from **{gld.Name}**");
                        await gld.AddBanAsync(user);
                        SocketTextChannel log = gld.GetChannel(423526041143738368) as SocketTextChannel;
                        await log.SendMessageAsync("", false, new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title = "__**Banned User**__",
                            Description = $"**{Author.Mention} banned user {user.Mention} from the server for reason:** {reason}",
                            ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
                            Timestamp = DateTime.Now,
                        }.Build());
                    }
                }
                else { await ReplyAsync("User does not exist in this guild"); }
            }
        }
        [Command("kick"), RequireBotPermission(GuildPermission.KickMembers), RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user = null, [Remainder] string reason = "")
        {
            SocketGuild gld = Context.Guild as SocketGuild;
            SocketRole mods = gld.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
            SocketRole owner = gld.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;
            SocketRole trialMod = gld.Roles.FirstOrDefault(x => x.Id == 675713190969081866) as SocketRole;
            SocketGuildUser Author = Context.User as SocketGuildUser;
            if (Author.Roles.Contains(mods) || Author.Roles.Contains(owner) || Author.Roles.Contains(trialMod) || !(Context.Channel.Id == 326078054948667393))
            {
                if (user.Id == 348505747828506624) { await ReplyAsync("How about, NO!"); }
                if (user == null) { await ReplyAsync("Please specify a user to kick"); return; }
                else
                {
                    await user.KickAsync();
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "__**Kicked User**__",
                        Color = new Color(0, 170, 230),
                        ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
                        Timestamp = DateTime.Now,
                        Description = $"{user.Mention} has been kicked from {gld.Name} for reason: {reason} by {Author.Mention}",
                    };
                    await ReplyAsync("", false, embed.Build());
                    SocketTextChannel log = gld.GetChannel(423526041143738368) as SocketTextChannel;
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
        }
        [Command("dam"), RequireUserPermission(GuildPermission.ManageMessages)] ///Needed User Permissions ///
        public async Task Clear(int Delete = 0, SocketGuildUser messageUser = null)
        {
            SocketGuild gld = Context.Guild as SocketGuild;
            SocketRole mods = gld.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
            SocketRole owner = gld.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;
            SocketRole trialMod = gld.Roles.FirstOrDefault(x => x.Id == 675713190969081866) as SocketRole;
            SocketGuildUser user = Context.Message.Author as SocketGuildUser;
            if (user.Roles.Contains(mods) || user.Roles.Contains(owner) || Context.User.Id == 257900591588573184 || user.Roles.Contains(trialMod) || !(Context.Channel.Id == 326078054948667393))
            {
                if (messageUser == null)
                {
                    if (Delete > 100) { await ReplyAsync("You cannot delete more than 100 messages"); }
                    var MessageToDelete = await Context.Channel.GetMessagesAsync(Delete + 1).FlattenAsync();
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(MessageToDelete);
                    Console.WriteLine($"Clear command was used to clear {Delete} messages by {user.Username}");
                    await (gld.GetChannel(423526041143738368) as SocketTextChannel).SendMessageAsync($"{Context.Message.Author.Mention} removed {Delete} messages from channel: {Context.Message.Channel.Name}");
                }
                else
                {
                    if (Delete > 100) { await ReplyAsync("You cannot delete more than 100 messages!"); }
                    var Messages = await Context.Channel.GetMessagesAsync(Delete + 1).FlattenAsync();
                    foreach (IMessage msg in Messages)
                    {
                        if (msg.Author.Id == messageUser.Id)
                        {
                            await msg.DeleteAsync();
                        }
                    }
                }
            }
            else { await ReplyAsync($"{user.Mention} sorry small thing. Permissions XD"); }

        }
        [Command("userinfo"), RequireBotPermission(GuildPermission.SendMessages)]
        public async Task UserInfo(SocketGuildUser user = null)
        {
            if (user == null) { await ReplyAsync("Please specifiy a user"); }
            else
            {
                string Roles = "";
                foreach (SocketRole role in user.Roles) { Roles = Roles + role.Mention.ToString(); }
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Title = $"Info for **{user.Nickname}**",
                    Color = new Color(0, 170, 230),
                    ThumbnailUrl = user.GetAvatarUrl(),
                    Description = $"**Username** - {user.Username}\n**Nickname** - {user.Nickname}\nDiscriminator - {user.Discriminator}\nCreated at - {user.CreatedAt}\nCurrent Status - {user.Status}\nJoined Server At - {user.JoinedAt}\nRoles - {Roles}"
                }.Build());
            }
        }
    }
}
