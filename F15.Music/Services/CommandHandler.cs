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

namespace F15.Services
{
    public class CommandHandler
    {
        // setup fields to be set later in the constructor
        private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private int newmsgs;

        public CommandHandler(IServiceProvider services)
        {
            // juice up the fields with these services
            // since we passed the services in, we can use GetRequiredService to pass them into the fields set earlier
            _config = services.GetRequiredService<IConfiguration>();
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();
            _services = services;

            // take action when we execute a command
            _commands.CommandExecuted += CommandExecutedAsync;

            // take action when we receive a message (so we can process it, and see if it is a valid command)
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            client.UserLeft += OnUserLeft;
            
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // this class is where the magic starts, and takes actions upon receiving messages
        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // ensures we don't process system/other bot messages
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            // sets the argument position away from the prefix we set
            var argPos = 0;

            // get prefix from the configuration file
            char prefix = Char.Parse(_config["Prefix"]);

            // determine if the message has a valid prefix, and adjust argPos based on prefix
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos)))
            {
                await DatabaseXPAsync(message);
                return;
            }

            var context = new SocketCommandContext(_client, message);

            // execute command if one is found that matches
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // if a command isn't found, log that info to console and exit this method
            if (!command.IsSpecified)
            {
                _logger.LogError($"Command failed to execute for [{context.User.Username}] <-> [{result.ErrorReason}]!");
                return;
            }


            // log success to the console and exit this method
            if (result.IsSuccess)
            {
                _logger.LogInformation($"Command [{command.Value.Name}] executed for [{context.User.Username}] on [{context.Guild.Name}]");
                return;
            }

            // failure scenario, let's let the user know
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
                    try { cont.Xp.Remove(useridtoremove); cont.SaveChanges(); } //Deletes the textfile which matches the user that left the server
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
    }
}