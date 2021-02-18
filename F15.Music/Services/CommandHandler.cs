using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq;
using Discord;
using System.IO;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Buzzard_V2;
using F15.Database;
using Victoria;
using System.Collections.Generic;
using F15.Music.Modules;

namespace F15.Services
{
    public class CommandHandler
    {
        private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private int newmsgs;
        public static List<SocketDMChannel> activeDMChannels = new List<SocketDMChannel>();
        public static SocketDMChannel activeDMChannel = null;
        public static List<List<ulong>> hangarUsers = new List<List<ulong>>();

        public CommandHandler(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfiguration>();
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;

            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            client.UserLeft += OnUserLeft;
            client.MessageReceived += messageRecieved;
            client.UserLeft += UserLeft;     //calls the annouce user left function when a user leaves the server
            client.UserJoined += UserJoined;    //Calls the user joined function when a user joins the server
            client.Ready += Event;
            client.ReactionAdded += ReactionsAdded;
            client.ReactionRemoved += ReactionsRemoved;
            client.UserBanned += UserBanned;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        private async Task Event()
        {
            await _client.SetGameAsync("DM me for support");
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            if (rawMessage.Channel is IPrivateChannel)
            {
                return;
            }


            var argPos = 0;

            char prefix = Char.Parse(_config["Prefix"]);

            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos)))
            {
                await DatabaseXPAsync(message);
                return;
            }

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
            {
                _logger.LogError($"Command failed to execute for [{context.User.Username}] <-> [{result.ErrorReason}]!");
                return;
            }


            if (result.IsSuccess)
            {
                _logger.LogInformation($"Command [{command.Value.Name}] executed for [{context.User.Username}] on [{context.Guild.Name}]");
                return;
            }

            await context.Channel.SendMessageAsync($"Sorry, {context.User.Username}... something went wrong -> [{result}]!");
        }

        public async Task OnUserLeft(SocketGuildUser user)
        {
            var bottesting = _client.GetChannel(374145773455278093) as ITextChannel;
            using (var cont = new XPContext())
            {
                if (cont.Xp.Any(o => o.DiscordId == user.Id.ToString()))
                {
                    var useridtoremove = cont.Xp.SingleOrDefault(o => o.DiscordId == user.Id.ToString());
                    try { cont.Xp.Remove(useridtoremove); cont.SaveChanges(); }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                    var embed = new EmbedBuilder();
                    embed.WithColor(new Color(0, 170, 230));
                    embed.ThumbnailUrl = "https://i.imgur.com/A1bgENa.png";
                    embed.Description = $"Removed {user.Username}'s file!";
                    await bottesting.SendMessageAsync("", false, embed.Build());
                }
            }
        }
        public async Task DatabaseXPAsync(SocketMessage msg)
        {
            try
            {
                SocketGuildUser user = msg.Author as SocketGuildUser;
                SocketGuild gld = _client.GetGuild(257984126718574592) as SocketGuild;
                if (user.Roles.Contains(gld.Roles.FirstOrDefault(x => x.Id == 301350337023836161) /*Guests*/) || user.Roles.Contains(gld.Roles.FirstOrDefault(x => x.Id == 301350531148808202)/*VIP*/) || msg.Channel.Id == 326078054948667393) { return; }
                else
                {
                    if (user.IsBot == true) { return; }
                    if (user.Roles.Count == 1 && user.Roles.Contains(gld.Roles.FirstOrDefault(x => x.Name.ToUpper() == "@EVERYONE"))) { return; }
                    var message = msg as SocketUserMessage;
                    Commands c = new Commands();
                    int usermsgs = 0;
                    if (!msg.Content.Contains("/"))
                    {
                        using (var cont = new XPContext())
                        {
                            if (!cont.Xp.Any(o => o.DiscordId == user.Id.ToString()))
                            {
                                await c.Insert(user);
                            }
                        }
                        using (var cont = new XPContext())
                        {
                            var data = await cont.Xp.FirstOrDefaultAsync(x => x.DiscordId == user.Id.ToString());
                            usermsgs = data.XpAmount;
                        }

                        newmsgs = usermsgs + 1;
                        using (var cont = new XPContext())
                        {
                            var userInfo = await cont.Xp.FirstOrDefaultAsync(b => b.DiscordId == user.Id.ToString());
                            userInfo.XpAmount = newmsgs;
                            cont.SaveChanges();
                        }

                        await c.LevelUpAsync(usermsgs, newmsgs, user, gld, msg.Channel as SocketTextChannel);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        private async Task messageRecieved(SocketMessage msg)
        {
            if (msg.Channel is IPrivateChannel && msg.Author.IsBot == false)
            {
                SocketGuild gld = _client.GetGuild(257984126718574592) as SocketGuild;
                if (!activeDMChannels.Contains(msg.Channel))
                {
                    activeDMChannels.Add(msg.Channel as SocketDMChannel);
                }
                if (activeDMChannel == null)
                    activeDMChannel = msg.Channel as SocketDMChannel;
                await (gld.GetChannel(686551634863849519) as ITextChannel).SendMessageAsync($"***{msg.Author.Username}** : {msg.Timestamp.Day}/{msg.Timestamp.Month}/{msg.Timestamp.Year} at {msg.Timestamp.Hour}:{msg.Timestamp.Minute} UTC*\n{msg.Content}");
            }
            else if (msg.Channel.Id == 686551634863849519 && msg.Author.IsBot == false && !msg.Content.Contains('/'))
            {
                if (activeDMChannel != null)
                {
                    await activeDMChannel.SendMessageAsync(msg.Content);
                }
            }
            else
            {
                string Message = msg.Content.ToLower();
                if ((msg.Channel.Id == 257984126718574592 || msg.Channel.Id == 426713281135116288) && (Message.Contains("rockstar") || Message.Contains("r*") || Message.Contains("rock*")))
                {
                    List<string> Replies = new List<string> { "Shark cards anyone?", "Anyone for shark cards?", "Shark cards lads only $100 for $800,000" };
                    await msg.Channel.SendMessageAsync(Replies[new Random().Next(0, Replies.Count)]);
                }
                else if (msg.Channel.Id == 492273478993444864)
                {
                    await (msg as SocketUserMessage).AddReactionAsync(new Emoji("👍"));
                    await (msg as SocketUserMessage).AddReactionAsync(new Emoji("👎"));
                }
            }
        }
        //-------Reaction Roles-----------
        //----Reactions Added
        private async Task ReactionsAdded(Cacheable<IUserMessage, ulong> Items, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuild gld = _client.GetGuild(257984126718574592);
            SocketRole role = null;
            var Guest = gld.GetRole(301350337023836161) as SocketRole;
            var CrewRecruit = gld.GetRole(428508411244707841) as SocketRole;
            var RulesProof = gld.GetRole(748100298144481290) as SocketRole;

            if (channel.Id == 311530747657715712) //Welcome channel
            {
                if (reaction.Emote.Name.ToLower() == "apply")
                {
                    SocketGuildUser user = gld.GetUser(reaction.UserId);
                    role = gld.GetRole(748100298144481290) as SocketRole; //RulesProof

                    if (user.Roles.Contains(RulesProof) != true)
                    {
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        if (user.Roles.Contains(Guest))
                        {
                            await AnnounceNewUser(reaction.UserId, gld.GetRole(301350337023836161), "Enjoy your stay here");
                            await LogUserJoined(reaction.UserId, gld, gld.GetRole(301350337023836161));
                        }
                        if (user.Roles.Contains(CrewRecruit))
                        {
                            await dmUser(reaction.UserId, gld);
                            await AnnounceNewUser(reaction.UserId, gld.GetRole(428508411244707841), "Enjoy your stay here");
                        }
                    }
                }
            }

            if (channel.Id == 522737339844395011) // Access-squadron-chats
            {

                switch (reaction.Emote.Name.ToLower())
                {
                    case "gtav":
                        role = gld.GetRole(426745071967141909) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "war_thunder":
                        role = gld.GetRole(426745457662885898) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "ac7":
                        role = gld.GetRole(540460922012172288) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "dcs":
                        role = gld.GetRole(676060833150926858) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "flightsim":
                        role = gld.GetRole(693797766689849385) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "sws":
                        role = gld.GetRole(732891813068668929) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                }
            }
            else if (channel.Id == 320600176328966145)// GTA Matchmaking
            {
                switch (reaction.Emote.Name.ToLower())
                {
                    case "lazer":
                        role = gld.GetRole(733974820097884190) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "buzzard":
                        role = gld.GetRole(733977242455244831) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                }
            }
            else if (channel.Id == 714511019761991771)
            {
                switch (reaction.Emote.Name.ToLower())
                {
                    case "refueling": //f16
                        role = gld.GetRole(787335567666708540) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "hornet": //f18
                        role = gld.GetRole(787335719332216852) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "harrier": //harrier
                        role = gld.GetRole(808968387371728927) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "brrt": //A10CII
                        role = gld.GetRole(787336850913493053) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "bf109": //Axis
                        role = gld.GetRole(787335871720325230) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "p51": //Allied
                        role = gld.GetRole(787336008102182933) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "mig21": //Mig
                        role = gld.GetRole(787336781078200390) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                    case "ka50": //Ka50
                        role = gld.GetRole(787336919133716501) as SocketRole;
                        await AddRole(role, reaction.UserId, gld, channel, true);
                        break;
                }
            }
            else if (channel.Id == 359339709824237568) //Hangar
            {

                int index = HangarModule.isInUserList(reaction.UserId);
                if (index == -1)
                {
                    HangarModule.AddNewUserToArray(reaction.UserId);
                    index = HangarModule.isInUserList(reaction.UserId);
                }
                SocketGuildUser user = gld.GetUser(reaction.UserId) as SocketGuildUser;
                SocketRole guest = gld.GetRole(301350337023836161);
                SocketRole recruits = gld.GetRole(428508411244707841);
                SocketRole pendingInspection = gld.GetRole(454537016818925569);
                switch (reaction.Emote.Name.ToLower())
                {
                    case "pilot":
                        if (index != -1)
                        {
                            if (hangarUsers[index].Contains(guest.Id)) { await reaction.Channel.SendMessageAsync("Role already saved!"); }
                            else
                            {
                                if (hangarUsers[index].Contains(recruits.Id))
                                {
                                    await reaction.Channel.SendMessageAsync("You can't have recruit and guest role at the same time !");
                                }
                                else
                                {
                                    hangarUsers[index].Add(guest.Id);
                                    await reaction.Channel.SendMessageAsync("Role saved please select a game.");
                                }
                            }
                        }
                        else { await reaction.Channel.SendMessageAsync("Please specify your name before choosing to be a guest."); }
                        break;
                    case "pcpi":
                        if (index != -1)
                        {
                            recruits = gld.GetRole(428508411244707841);
                            guest = gld.GetRole(301350337023836161);
                            if (hangarUsers[index].Contains(recruits.Id)) { await reaction.Channel.SendMessageAsync("Role already saved!"); }
                            else
                            {
                                if (hangarUsers[index].Contains(guest.Id))
                                {
                                    await reaction.Channel.SendMessageAsync("You can't have guest and recruit role at the same time !");
                                }
                                else
                                {
                                    string nickname = user.Username;
                                    if (user.Nickname != null) { nickname = user.Nickname; }
                                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).ModifyAsync(x => x.Nickname = $"[N/A] {nickname}");
                                    hangarUsers[index].Add(recruits.Id);
                                    hangarUsers[index].Add(pendingInspection.Id);
                                    await reaction.Channel.SendMessageAsync("Role saved please select a game.");
                                }
                            }
                        }
                        else { await reaction.Channel.SendMessageAsync("Please specify your name before choosing to be a member."); }
                        break;
                    case "dcs":
                        if (index != -1)
                        {
                            SocketRole dcs = gld.GetRole(676060833150926858);
                            pendingInspection = gld.GetRole(799264189545054209);
                            if (hangarUsers[index].Contains(dcs.Id)) { await reaction.Channel.SendMessageAsync("Role already saved."); }
                            else
                            {
                                hangarUsers[index].Add(dcs.Id);
                                hangarUsers[index].Add(pendingInspection.Id);
                                await reaction.Channel.SendMessageAsync("Role saved.");
                            }
                        }
                        break;
                    case "gtav":
                        if (index != -1)
                        {
                            SocketRole GTAV = gld.GetRole(426745071967141909);
                            pendingInspection = gld.GetRole(454537016818925569); //pending inspection GTA
                            if (hangarUsers[index].Contains(GTAV.Id)) { await reaction.Channel.SendMessageAsync("Role already saved."); }
                            else
                            {
                                hangarUsers[index].Add(GTAV.Id);
                                hangarUsers[index].Add(pendingInspection.Id);
                                await reaction.Channel.SendMessageAsync("Role saved.");
                            }
                        }
                        break;
                    case "war_thunder":
                        if (index != -1)
                        {
                            SocketRole warthunder = gld.GetRole(426745457662885898);
                            pendingInspection = gld.GetRole(799264318570889217);
                            if (hangarUsers[index].Contains(warthunder.Id)) { await reaction.Channel.SendMessageAsync("Role already saved."); }
                            else
                            {
                                hangarUsers[index].Add(warthunder.Id);
                                hangarUsers[index].Add(pendingInspection.Id);
                                await reaction.Channel.SendMessageAsync("Role saved.");
                            }
                        }
                        break;
                    case "ac7":
                        if (index != -1)
                        {
                            SocketRole ac7 = gld.GetRole(540460922012172288);
                            if (hangarUsers[index].Contains(ac7.Id)) { await reaction.Channel.SendMessageAsync("Role already saved."); }
                            else
                            {
                                hangarUsers[index].Add(ac7.Id);
                                await reaction.Channel.SendMessageAsync("Role saved.");
                            }
                        }
                        break;
                    case "flightsim":
                        if (index != -1)
                        {
                            SocketRole flightsim = gld.GetRole(693797766689849385);
                            if (hangarUsers[index].Contains(flightsim.Id)) { await reaction.Channel.SendMessageAsync("Role already saved."); }
                            else
                            {
                                hangarUsers[index].Add(flightsim.Id);
                                await reaction.Channel.SendMessageAsync("Role saved.");
                            }
                        }
                        break;
                    case "sws":
                        if (index != -1)
                        {
                            SocketRole sws = gld.GetRole(732891813068668929);
                            if (hangarUsers[index].Contains(sws.Id)) { await reaction.Channel.SendMessageAsync("Role already saved."); }
                            else
                            {
                                hangarUsers[index].Add(sws.Id);
                                await reaction.Channel.SendMessageAsync("Role saved.");
                            }
                        }
                        break;
                    case "apply":
                        if (index != -1)
                        {
                            List<ulong> userDetails = hangarUsers[index];
                            foreach (ulong a in userDetails)
                            {
                                switch (a)
                                {
                                    case 301350337023836161: //Guest
                                        for (int i = 1; i < userDetails.Count; i++)
                                        {
                                            await AddRole(gld.GetRole(userDetails[i]), reaction.UserId, gld, reaction.Channel, false);
                                        }
                                        //await AnnounceNewUser(reaction.UserId, gld.GetRole(301350337023836161), "Enjoy your stay here");
                                        //await LogUserJoined(reaction.UserId, gld, gld.GetRole(301350337023836161));
                                        await HangarModule.HangarDeleteMessages(reaction.Channel as SocketTextChannel);
                                        hangarUsers.RemoveAt(index);
                                        goto end;

                                    case 428508411244707841: //Crew recruit
                                        for (int i = 1; i < userDetails.Count; i++)
                                        {
                                            await AddRole(gld.GetRole(userDetails[i]), reaction.UserId, gld, reaction.Channel, false);
                                        }
                                        //await dmUser(reaction.UserId, gld);
                                        //await AnnounceNewUser(reaction.UserId, gld.GetRole(428508411244707841), "Enjoy your stay here");
                                        await HangarModule.HangarDeleteMessages(reaction.Channel as SocketTextChannel);
                                        hangarUsers.RemoveAt(index);
                                        goto end;
                                }
                            }
                        end:
                            Console.WriteLine();
                        }
                        else { }
                        break;
                }
            }
        }
        private async Task AddRole(SocketRole role, ulong userId, SocketGuild gld, ISocketMessageChannel channel, bool message)
        {
            SocketGuildUser user = gld.GetUser(userId);
            ITextChannel Log = gld.GetTextChannel(423526041143738368);
            if (role.Id == 748100298144481290)
            {
                //Welcome Rules
                if (user.Roles.Contains(role))
                {
                    //var msg = await channel.SendMessageAsync($"{user.Username} You already have the game role");
                    //await Task.Delay(2000);
                    //await msg.DeleteAsync();
                    //return;
                }
                else
                {
                    await user.AddRoleAsync(role);

                    if (message == true)
                    {
                        //var msg = await channel.SendMessageAsync($"{user.Username} has been assigned the game role: **{role.Name}**");
                        //await Task.Delay(2000);
                        //await msg.DeleteAsync();
                        await Log.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = "Game role added",
                            Description = $"<@{userId}> has added role {role.Mention}",
                            Color = role.Color,
                        }.Build());
                    }

                }
            }
            else
            {
                if (user.Roles.Contains(role))
                {
                    var msg = await channel.SendMessageAsync($"{user.Mention} You already have the game role");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                    return;
                }
                else
                {
                    await user.AddRoleAsync(role);
                    if (role.Id == 748100298144481290)
                    {
                        //Welcome Rules
                    }
                    else if (message == true)
                    {
                        var msg = await channel.SendMessageAsync($"{user.Mention} has been assigned the game role: **{role.Name}**");
                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await Log.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = "Game role added",
                            Description = $"<@{userId}> has added role {role.Mention}",
                            Color = role.Color,
                        }.Build());
                    }
                }
            }
        }
        //----Reactions Removed
        private async Task ReactionsRemoved(Cacheable<IUserMessage, ulong> Items, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuild gld = _client.GetGuild(257984126718574592);
            if (channel.Id == 522737339844395011)
            {
                ITextChannel Log = gld.GetTextChannel(423526041143738368);
                if (reaction.Emote.Name.ToLower() == "gtav")
                {
                    SocketRole role = gld.GetRole(426745071967141909) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "war_thunder")
                {
                    SocketRole role = gld.GetRole(426745457662885898) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "ac7")
                {
                    SocketRole role = gld.GetRole(540460922012172288) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "dcs")
                {
                    SocketRole role = gld.GetRole(676060833150926858) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "flightsim")
                {
                    SocketRole role = gld.GetRole(693797766689849385) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "sws")
                {
                    SocketRole role = gld.GetRole(732891813068668929) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
            }
            else if (channel.Id == 714511019761991771)
            {
                ITextChannel Log = gld.GetTextChannel(423526041143738368);
                if (reaction.Emote.Name.ToLower() == "refueling")
                {
                    SocketRole role = gld.GetRole(787335567666708540) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "hornet")
                {
                    SocketRole role = gld.GetRole(787335719332216852) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "harrier")
                {
                    SocketRole role = gld.GetRole(808968387371728927) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "brrt")
                {
                    SocketRole role = gld.GetRole(787336850913493053) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "bf109")
                {
                    SocketRole role = gld.GetRole(787335871720325230) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "p51")
                {
                    SocketRole role = gld.GetRole(787336008102182933) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "mig21")
                {
                    SocketRole role = gld.GetRole(787336781078200390) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
                else if (reaction.Emote.Name.ToLower() == "ka50")
                {
                    SocketRole role = gld.GetRole(787336919133716501) as SocketRole;
                    await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(role);
                    var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                    await Task.Delay(2000);
                    await msg.DeleteAsync();
                }
            }
            else if (channel.Id == 320600176328966145)
            {
                switch (reaction.Emote.Name.ToLower())
                {
                    case "lazer":
                        SocketRole JetDF = gld.GetRole(733974820097884190) as SocketRole;
                        await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(JetDF);
                        var a = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                        await Task.Delay(2000);
                        await a.DeleteAsync();
                        break;
                    case "buzzard":
                        SocketRole HeliDF = gld.GetRole(733977242455244831) as SocketRole;
                        await (gld.GetUser(reaction.UserId) as SocketGuildUser).RemoveRoleAsync(HeliDF);
                        var msg = await channel.SendMessageAsync($"{(gld.GetUser(reaction.UserId) as SocketGuildUser).Mention} role removed.");
                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        break;
                }
            }
            else if (channel.Id == 359339709824237568)
            {
                int index = HangarModule.isInUserList(reaction.UserId);
                SocketRole pendingInspection = gld.GetRole(454537016818925569);
                if (index != -1)
                {
                    switch (reaction.Emote.Name.ToLower())
                    {
                        case "gtav":
                            SocketRole gtav = gld.GetRole(426745071967141909);
                            pendingInspection = gld.GetRole(454537016818925569);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(gtav.Id));
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(pendingInspection.Id));
                            await reaction.Channel.SendMessageAsync("GTAV Role removed.");
                            break;
                        case "war_thunder":
                            SocketRole wt = gld.GetRole(426745457662885898);
                            pendingInspection = gld.GetRole(799264318570889217);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(wt.Id));
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(pendingInspection.Id));
                            await reaction.Channel.SendMessageAsync("War Thunder Role removed.");
                            break;
                        case "dcs":
                            SocketRole dcs = gld.GetRole(676060833150926858);
                            pendingInspection = gld.GetRole(799264189545054209);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(dcs.Id));
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(pendingInspection.Id));
                            await reaction.Channel.SendMessageAsync("DCS Role removed.");
                            break;
                        case "ac7":
                            SocketRole ac7 = gld.GetRole(540460922012172288);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(ac7.Id));
                            await reaction.Channel.SendMessageAsync("Ace Combat Role removed.");
                            break;
                        case "flightsim":
                            SocketRole flightsim = gld.GetRole(693797766689849385);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(flightsim.Id));
                            await reaction.Channel.SendMessageAsync("Flight Simulator Role removed.");
                            break;
                        case "sws":
                            SocketRole sws = gld.GetRole(732891813068668929);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(sws.Id));
                            await reaction.Channel.SendMessageAsync("Star Wars Squadrons Role removed.");
                            break;
                        case "pcpi":
                            SocketRole recruits = gld.GetRole(428508411244707841);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(recruits.Id));
                            await reaction.Channel.SendMessageAsync("Member role removed.");
                            string newNickname = gld.GetUser(reaction.UserId).Nickname.Replace("[N/A]", "");
                            await gld.GetUser(reaction.UserId).ModifyAsync(x => x.Nickname = newNickname);
                            break;
                        case "pilot":
                            SocketRole guest = gld.GetRole(301350337023836161);
                            hangarUsers[index].RemoveAt(hangarUsers[index].IndexOf(guest.Id));
                            await reaction.Channel.SendMessageAsync("Guest role removed.");
                            break;
                    }
                }
            }
        }
        //Other Tasks

        private async Task UserBanned(SocketUser user, SocketGuild gld)
        {
            SocketTextChannel log = _client.GetChannel(423526041143738368) as SocketTextChannel;
            await log.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = "**User Banned**",
                Color = Color.Red,
                ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
                Description = $"{user.Username} was banned from {gld.Name}",
            }.Build());
        }
        private async Task UserJoined(SocketGuildUser user)
        {
            if (user.IsBot == true)
            {
                SocketTextChannel log = _client.GetChannel(423526041143738368) as SocketTextChannel;
                await log.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = $"{user.Username} has been kicked from {user.Guild.Name} as they are a bot",
                    Color = Color.Red,
                    ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1"
                }.Build());
                await user.KickAsync();
            }
            else
            {
                ITextChannel modsChannel = _client.GetChannel(688354508002164746) as ITextChannel;
                await modsChannel.SendMessageAsync($"{_client.GetGuild(257984126718574592).GetRole(311889967679012864).Mention} {user.Mention} has joined the server");
            }
        }
        private async Task UserLeft(SocketGuildUser user)
        {
            SocketTextChannel log = _client.GetChannel(423526041143738368) as SocketTextChannel;
            SocketTextChannel channel = _client.GetChannel(755443198192648362) as SocketTextChannel;
            SocketGuild gld = _client.GetGuild(257984126718574592) as SocketGuild;
            if (!(user.Roles.Contains(gld.Roles.FirstOrDefault(x => x.Name.ToUpper() == "@EVERYONE")) && user.Roles.Count == 1))
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(0, 170, 230));
                embed.Title = $"{user.Username} has left PC Pilots Air Space";
                embed.ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1";
                await channel.SendMessageAsync("", false, embed.Build());
                await log.SendMessageAsync($"**{user.Nickname} / {user.Username} has left the server!** ID = {user.Id}");
            }
        }
        private async Task dmUser(ulong userId, SocketGuild gld)
        {
            SocketGuildUser user = gld.GetUser(userId);
            if (user.Roles.Contains(gld.GetRole(428508411244707841)))
            {
                await UserExtensions.SendMessageAsync(user, "", false, new EmbedBuilder()
                {
                    Title = "Welcome to the PC Pilots Crew Discord Server!",
                    Description = "**Please read #welcome**, located left, at the top of the channel list.\n\n" +
                    "If you have any questions regarding the crew, feel free to mention our server moderators, using **@Moderators**.\n\n" +
                    "We are glad to have you as a new crew applicant. Please be aware that **you must qualify to become a full member!**\n\n" +
                    "You can find all info here: \n https://pcpilotscrew.com/join-our-crew \n https://pcpilotscrew.com/ranking-system \n\n" +
                    "Have a nice flight!",
                    ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
                    Color = new Color(0, 170, 230)
                }.Build());
            }
        }
        private async Task LogUserJoined(ulong userId, SocketGuild gld, SocketRole role)
        {
            SocketTextChannel log = gld.GetChannel(423526041143738368) as SocketTextChannel;
            SocketGuildUser user = gld.GetUser(userId);
            await log.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = $"{user.Username} joined the server",
                Color = new Color(0, 170, 230),
                ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
                Description = $"{user.Username} assigned themselves as a {role.Mention} with the nickname of: **{user.Nickname}**",
                Timestamp = DateTime.Now,

            }.Build());
        }
        public async Task AnnounceNewUser(ulong userID, SocketRole role, string message)
        {
            try
            {
                SocketGuild gld = _client.GetGuild(257984126718574592) as SocketGuild;
                SocketGuildUser user = gld.GetUser(userID);
                ITextChannel channel = gld.GetChannel(755443198192648362) as ITextChannel;
                SocketTextChannel general = gld.GetChannel(257984126718574592) as SocketTextChannel;
                SocketTextChannel welcome = gld.GetChannel(311530747657715712) as SocketTextChannel;
                await channel.SendMessageAsync(user.Mention);
                await channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Title = $"{user.Username} has infiltrated PC Pilots Air Space",
                    Color = new Color(0, 170, 230),
                    ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1",
                    Description = $"{user.Mention} please read {welcome.Mention}.\nYou have been given the role of {role.Mention}\nPlease say Hello in {general.Mention}.\n{message}"
                }.Build());
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
    }
}