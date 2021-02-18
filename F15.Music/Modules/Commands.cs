using Discord;
using Discord.Commands;
using Discord.WebSocket;
using F15;
using F15.Database;
using F15.Services;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Buzzard_V2
{
    public class Commands : ModuleBase
    {
        private static readonly string directory = @"C:\Users\Administrator\Desktop\Bots\Bot-Data\";
        private int usermsgs;

        [Command("Insert")]
        public async Task InsertDB(SocketGuildUser user)
        {
            if ((Context.User as SocketGuildUser).Id != 257900591588573184)
            {
                Console.WriteLine("What did you think was going to happen when you entered this command?");
            }
            else
            {
                await Insert(user);
            }
        }
        public async Task Insert(SocketGuildUser user)
        {
            using (var cont = new XPContext())
            {
                var xp = await cont.Xp.ToListAsync();
                cont.Xp.Add(new Xp() { DiscordId = user.Id.ToString(), XpAmount = 0 });
                cont.SaveChanges();
            }
        }
        [Command("ListXp")]
        public async Task ListXp(string id = null)
        {
            if ((Context.User as SocketGuildUser).Id != 257900591588573184)
            {
                Console.WriteLine("What did you think was going to happen when you entered this command?");
            }
            else
            {
                if (id != null)
                {
                    using (var cont = new XPContext())
                    {
                        var data = await cont.Xp.FirstOrDefaultAsync(x => x.DiscordId == id);
                        int truexpamount = data.XpAmount * 12;
                        await ReplyAsync(truexpamount.ToString());
                    }
                }
                else
                {
                    using (var cont = new XPContext())
                    {
                        var data = await cont.Xp.FirstOrDefaultAsync(x => x.DiscordId == Context.User.Id.ToString());
                        int truexpamount = data.XpAmount * 12;
                        await ReplyAsync(truexpamount.ToString());
                    }
                }
            }
        }
        [Command("AlterXp")]
        public async Task AlterXp(double xp = 0, SocketGuildUser user = null)
        {
            if ((Context.User as SocketGuildUser).Id != 257900591588573184)
            {
                Console.WriteLine("What did you think was going to happen when you entered this command?");
            }
            else
            {
                if (user != null || xp != 0)
                {
                    double truexp = Math.Round(xp / 12, 0, MidpointRounding.AwayFromZero);
                    using (var cont = new XPContext())
                    {
                        var userInfo = await cont.Xp.FirstOrDefaultAsync(b => b.DiscordId == user.Id.ToString());
                        userInfo.XpAmount = userInfo.XpAmount += Convert.ToInt32(truexp);
                        cont.SaveChanges();
                    }
                    await ReplyAsync($"{Context.User} has given {user} {xp} xp");
                }
                else
                {
                    await ReplyAsync("Wrong Command syntax, make sure to use /AlterXp [xp] [@user]");
                }
            }
        }


        [Command("givexp")]
        public async Task GiveXP(double xp = 0, SocketGuildUser user1 = null, SocketGuildUser user2 = null, SocketGuildUser user3 = null, SocketGuildUser user4 = null, SocketGuildUser user5 = null)
        {
            try
            {
                if (xp == 0 || user1 == null) { await Context.Channel.SendMessageAsync("Please ensure that you specify the amount of XP to give or the user to give XP. syntax: /givexp [xp] [@user] up to 5 users only"); return; }
                SocketGuild gld = Context.Guild as SocketGuild;
                SocketRole mods = gld.GetRole(311889967679012864) as SocketRole;
                SocketRole Owner = gld.GetRole(257984497071554560) as SocketRole;
                SocketRole trialMods = gld.GetRole(675713190969081866) as SocketRole;
                SocketRole squadLeader = gld.GetRole(311526363196817408) as SocketRole;
                if ((Context.User as SocketGuildUser).Roles.Contains(mods) || (Context.User as SocketGuildUser).Roles.Contains(Owner) || (Context.User as SocketGuildUser).Roles.Contains(trialMods) || (Context.User as SocketGuildUser).Roles.Contains(squadLeader) || (Context.User as SocketGuildUser).Id == 257900591588573184)
                {
                    SocketRole Guest = gld.GetRole(301350337023836161) as SocketRole;
                    ITextChannel Log = gld.GetChannel(423526041143738368) as ITextChannel;
                    string LogMessage = "";
                    if (!(user1.Roles.Contains(Guest)))
                    {
                        await AddXP(xp, user1);
                        LogMessage = $"{user1.Mention} has been awarded **{xp}xp** by: {Context.User}";
                    }
                    if (!(user2 == null))
                    {
                        if (!(user2.Roles.Contains(Guest)))
                        {
                            await AddXP(xp, user2);
                            LogMessage = LogMessage + $"\n{user2.Mention} has been awarded **{xp}xp** by: {Context.User}";
                        }
                    }
                    if (!(user3 == null))
                    {
                        if (!(user3.Roles.Contains(Guest)))
                        {
                            await AddXP(xp, user3);
                            LogMessage = LogMessage + $"\n{user3.Mention} has been awarded **{xp}xp** by: {Context.User}";
                        }
                    }
                    if (!(user4 == null))
                    {
                        if (!(user4.Roles.Contains(Guest)))
                        {
                            await AddXP(xp, user4);
                            LogMessage = LogMessage + $"\n{user4.Mention} has been awarded **{xp}xp** by: {Context.User}";
                        }
                    }
                    if (!(user5 == null))
                    {
                        if (!(user5.Roles.Contains(Guest)))
                        {
                            await AddXP(xp, user5);
                            LogMessage = LogMessage + $"\n{user5.Mention} has been awarded **{xp}xp** by: {Context.User}";
                        }
                    }
                    await Log.SendMessageAsync(LogMessage);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); await ReplyAsync("Error in function GiveXP()"); }
        }
        public async Task AddXP(double xp, SocketGuildUser user)
        {
            SocketGuild gld = Context.Guild as SocketGuild;
            using (var cont = new XPContext())
            {
                if (!cont.Xp.Any(o => o.DiscordId == user.Id.ToString()))
                {
                    await Insert(user);
                }
            }
            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            int usermsgs;
            double truexp = Math.Round(xp / 12, 0, MidpointRounding.AwayFromZero);
            using (var cont = new XPContext())
            {
                var userInfo = await cont.Xp.FirstOrDefaultAsync(b => b.DiscordId == user.Id.ToString());
                userInfo.XpAmount = userInfo.XpAmount += Convert.ToInt32(truexp);
                cont.SaveChanges();
                usermsgs = userInfo.XpAmount - Convert.ToInt32(truexp);
            }
            int addedmsgs = usermsgs + Convert.ToInt32(truexp);
            await ReplyAsync($"{user.Mention} has been rewarded {xp} XP by: `{Context.User}`");
            await LevelUpAsync(usermsgs, addedmsgs, user, gld, channel); //pass the premessages, addedmessages, the socketmessage and the guild
        }

        [Command("cat"), Summary("Grabs a random cat image.")]  //This is like the gimme cats command, but it only sends one cat image and not to the user's DM but to the command channel
        public async Task ACat(string input = "")
        {
            if (input == "" || input != "")
            {
                HttpResponseMessage response = await new HttpClient().GetAsync("http://thecatapi.com/api/images/get?format=xml&results_per_page=20");   //if it fails to get the xml, then we return  
                
                if (!response.IsSuccessStatusCode) { await ReplyAsync("Failed to get response from server, please try again"); return; };                  //Checks if there was a response

                string xml = await response.Content.ReadAsStringAsync();    //read the xml as a string
                XDocument xdoc = XDocument.Parse(xml);      //parse xml                                              
                var elems = xdoc.Elements("response").Elements("data").Elements("images").Elements("image");

                if (elems == null) return;                  //if its null, then we return

                List<string> urls = new List<string>();     //new list to store the urls
                urls.AddRange(elems.Select(x => x.Element("url")?.Value));  //add the image urls to the list
                int max = urls.Count - 1;                //set the max page and reduce by 1 cause array starts from 0       

                await ReplyAsync("", false, new EmbedBuilder
                {  //Send message and create embed
                    Title = "Cat Image",                        //Title for embed
                    Color = new Color(0, 170, 230),             //Color for embed
                    ImageUrl = urls.First(),                    //Cat image URL 
                }.Build());
            }
        }
        [Command("fire")]   //Fire command, another fun command
        public async Task Fire([Remainder] string user = null)
        {
            if (Context.Channel.Id == 326078054948667393) { return; }
            if (user == null) { await ReplyAsync("Who do you want me to shoort at?"); }
            else if (user == "<@366986823274201089>")
            {
                var LazerReplies = new string[] { "Why do you make me do this?", "Ahhh fine!", "This is how your treat the bots ayyy? ", "Really M9 making me fire at myself", "<@366986823274201089> committed suicide" };
                string reply = LazerReplies[new Random().Next(0, LazerReplies.Length)];
                await ReplyAsync(reply);
            }
            else if (user == "@here" || user == "@everyone") { await ReplyAsync($"Shots were fired at {Context.User.Mention}!"); }
            else { await ReplyAsync($"Shots were fired at {user}"); }
        }
        [Command("smile")] //Smile command that literally is another random output :D                                        
        public async Task Smile([Remainder] string input = "")
        {
            if (input == "" || input != "")
            {
                if (Context.Channel.Id == 326078054948667393) { return; }
                var replies = new string[] { "Sure ", "Okay! ", "Fine ", "Ok ", "here you go " };   //the different replies the bot can send
                string reply = replies[new Random().Next(0, replies.Length)];                       //chooses randomly through the array
                await ReplyAsync(reply + ":smiley:");                                               //most random command ever
            }
        }

        [Command("help")]
        public async Task Help([Remainder] string input = "")
        {
            if (Context.Channel.Id == 326078054948667393) { return; }   //Checks if the channel si badsport
            input = input.ToLower();
            EmbedBuilder embed = new EmbedBuilder();
            SocketRole owner = Context.Guild.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;
            SocketRole mods = Context.Guild.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
            SocketGuildUser Author = Context.User as SocketGuildUser;

            embed.WithColor(new Color(0, 170, 230));
            embed.Title = "__**Main Commands**__";
            embed.Description = File.ReadAllText($"OverallList.txt");
            embed.ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1";
            embed.WithFooter("Type /help followed by a bots name for detailed commands related to them");
            await ReplyAsync("", false, embed.Build());

        }
        [Command("rules")]  //Rules command displays the rules for a specified topic. The topic is passed as an input parameter by the suer as the variable "rules"
        public async Task Rules()
        {
            await ReplyAsync("https://pcpilotscrew.com/rules-and-guidelines");
        }

        [Command("links")]
        public async Task Links([Remainder] string input = "")
        {
            if (Context.Channel.Id == 326078054948667393) { return; }
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(0, 170, 230));
            embed.ThumbnailUrl = "https://i0.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/pcpi2_ytlogo512.png?resize=120%2C120&ssl=1";
            embed.Title = "__**Social Links**__";
            embed.Description = "[PC Pilots Website](https://pcpilotscrew.com/)\n\n[YouTube](https://www.youtube.com/channel/UCd8NZO_JkFms2pvyxxe1Flg)\n\n[Twitch](https://www.twitch.tv/pcpilotscrew)\n\n[Discord](https://discord.gg/mu3cvWA)\n\n[Instagram](https://www.instagram.com/pcpilotscrew/)\n\n[Social Club Crew](https://socialclub.rockstargames.com/crew/pc_pilots_crew/wall)\n\n[Ace Combat Group](https://steamcommunity.com/groups/pcpilotsac)\n\n[Twitter](https://twitter.com/pcpilotscrew)\n\n[Patreon](https://www.patreon.com/pcpilots)";
            await ReplyAsync("", false, embed.Build());
        }

        [Command("8ball")]
        public async Task Ball([Remainder] string Question = "")    //Very simple comand really just sends a random output, ok next command
        {
            if (Question != "")
            {
                List<string> Replies = new List<string> { "Yes", "Maybe :thinking:", "Nah M8", "Gucci", "Who knows", "Possibly", "IDK FEK OFF!", "Hell yes", "Definitely" };
                await ReplyAsync(Replies[new Random().Next(0, Replies.Count)]);
            }
            else { await ReplyAsync("What is your question?"); }
        }
        [Command("Flipcoin")]   //Flipa  coin command. Simple random number generator which determines an output. Which is either heads or tails
        public async Task FlipCoin([Remainder] string input)
        {
            int Chance = new Random().Next(2);  //declares random number
            EmbedBuilder Embed = new EmbedBuilder();    //Makes custom embed
            Embed.Color = new Color(0, 170, 230);
            if (Chance == 0)
            {
                Embed.Title = "Heads";
                Embed.ImageUrl = "https://researchmaniacs.com/Random/Images/Quarter-Heads.png";
            }
            else if (Chance == 1)
            {
                Embed.Title = "Tails";
                Embed.ImageUrl = "https://wi-images.condecdn.net/image/kMw9dD3kMqD/crop/900/f/pound-coin.png";
            }
            await ReplyAsync("", false, Embed.Build());       //Sends embed   
        }
        [Command("avatar")] //Gets a specified user's avatar, because why not
        public async Task Avatar(SocketGuildUser user = null)
        {
            if (user != null)   //Checks if the user is null
            {
                await ReplyAsync("", false, new EmbedBuilder
                {  //Declares custom emebed then sends it
                    Title = user.Nickname,
                    Color = new Color(0, 170, 230),
                    ImageUrl = user.GetAvatarUrl(),
                }.Build());
            }
            else { await ReplyAsync("Please specify a user. Syntax /avatar user"); }    //Output message if the user is null
        }
        [Command("gimmecats")]  //Gimme cats command, sends a specified number of random cat images of GIFs to a user's DM
        public async Task GimmeCats(int Cats = 0)
        {
            if (Context.Channel.Id == 326078054948667393) { return; }
            if (Cats == 0) { await ReplyAsync("How many cats do you want?"); return; }  //Checks if the user entered the number of cats they want
            if (Cats <= 10) //Checks if the number of cats is less than 10.
            {
                await ReplyAsync($"{Context.User.Mention} cats in your DM");    //Sends message to command channel to inform the user to check their DM
                EmbedBuilder embed = new EmbedBuilder();    //Creates custom embed               
                var response = await new HttpClient().GetAsync("http://thecatapi.com/api/images/get?format=xml&results_per_page=20");
                //if it fails to get the xml, then we return
                if (!response.IsSuccessStatusCode) return;
                string xml = await response.Content.ReadAsStringAsync();
                var xdoc = XDocument.Parse(xml); //parse xml                                              
                var elems = xdoc.Elements("response").Elements("data").Elements("images").Elements("image");
                if (elems == null) return; //if its null, then we return
                List<string> urls = new List<string>(); //new list to store the urls
                urls.AddRange(elems.Select(x => x.Element("url")?.Value)); //add the image urls to the list
                int max = urls.Count - 1; //set the max page and reduce the value by 1 since and array starts at 0               
                for (int i = 0; i < Cats; i++)
                {
                    embed.WithTitle($"Cat Image"); //set title               
                    embed.WithColor(new Color(0, 170, 230)); //set color
                    embed.WithCurrentTimestamp(); //timestamp              
                    embed.WithImageUrl(urls[i]); //set the first image
                    await UserExtensions.SendMessageAsync(Context.User, "", false, embed.Build());
                }
            }
            else { await ReplyAsync("Number of cats must be less than 10"); }
        }

        public async Task LevelUpAsync(int PreMsgs, int addedmsgs, SocketGuildUser user, SocketGuild gld, SocketTextChannel channel)
        {
            try
            {
                //The levels ---> 10, 25, 40, 60, 80, 105, 130, 160, 190, 225, 260, 300, 340, 385, 430, 475, 510, 560, 610, 660, 710, 760, 800, 1000, 1250, 1500, 1750, 2250, 2750, 3250, 3750, 4500, 5000, 5500, 6500, 8000           
                string rank = "Airman Basic";
                int xp = 0;
                string rewards = "None";
                string tag = "None";
                string badgeUrl = "https://i.imgur.com/A1bgENa.png";
                bool leveledUp = false;
                SocketRole YouTubeTeam = gld.Roles.FirstOrDefault(x => x.Id == 301351461424594957) as SocketRole;
                SocketRole Staff = gld.Roles.FirstOrDefault(x => x.Id == 257984752236101643) as SocketRole;
                SocketRole Leader = gld.Roles.FirstOrDefault(x => x.Id == 301350078151393283) as SocketRole;
                SocketRole Owner = gld.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;
                SocketRole CrewRecruit = gld.Roles.FirstOrDefault(x => x.Id == 428508411244707841) as SocketRole;
                if (!(user.Roles.Contains(Owner) || user.Roles.Contains(Leader) || user.Roles.Contains(YouTubeTeam) || user.Id == 175673821171286016))
                {
                    if (PreMsgs <= 10 && addedmsgs > 10)
                    {
                        leveledUp = true;
                        rank = "Airman Basic II";
                        badgeUrl = "https://i.imgur.com/A1bgENa.png";
                        tag = "[N/A]";
                    }
                    if (PreMsgs <= 25 && addedmsgs > 25)
                    {
                        leveledUp = true;
                        rank = "Airman Basic III";
                        badgeUrl = "https://i.imgur.com/A1bgENa.png";
                        tag = "[N/A]";
                    }
                    if (PreMsgs <= 40 && addedmsgs > 40)
                    {
                        leveledUp = true;
                        rank = "Airman Basic IV";
                        badgeUrl = "https://i.imgur.com/A1bgENa.png";
                        tag = "[N/A]";
                    }
                    if (PreMsgs <= 60 && addedmsgs > 60)
                    {
                        var member = gld.Roles.FirstOrDefault(x => x.Id == 339703419537457175) as SocketRole;
                        leveledUp = true;
                        rank = "Airman";
                        badgeUrl = "https://i2.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/PCPI_Ranks_V2_1.png?resize=120%2C120";
                        tag = "[Amn]";
                        rewards = $"{member.Mention} role added";
                        await user.AddRoleAsync(member);
                        await user.RemoveRoleAsync(CrewRecruit);
                    }
                    if (PreMsgs <= 80 && addedmsgs > 80)
                    {
                        leveledUp = true;
                        rank = "Airman II";
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                        tag = "[Amn]";
                    }
                    if (PreMsgs <= 105 && addedmsgs > 105)
                    {
                        leveledUp = true;
                        rank = "Airman III";
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                        tag = "[Amn]";
                    }
                    if (PreMsgs <= 130 && addedmsgs > 130)
                    {
                        leveledUp = true;
                        rank = "Airman IV";
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                        tag = "[Amn]"; ;
                    }
                    if (PreMsgs <= 160 && addedmsgs > 160)
                    {
                        leveledUp = true;
                        rank = "Airman 1st Class";
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                        tag = "[A1C]";
                    }
                    if (PreMsgs <= 190 && addedmsgs > 190)
                    {
                        leveledUp = true;
                        rank = "Airman 1st Class I";
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                        tag = "[A1C]";
                    }
                    if (PreMsgs <= 225 && addedmsgs > 225)
                    {
                        leveledUp = true;
                        rank = "Airman 1st Class II";
                        badgeUrl = "https://i.imgur.com/LAXe36a.png0";
                        tag = "[A1C]";
                    }
                    if (PreMsgs <= 260 && addedmsgs > 260)
                    {
                        leveledUp = true;
                        rank = "Airman 1st Class IV";
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                        tag = "[A1C]";
                    }
                    if (PreMsgs <= 300 && addedmsgs > 300)
                    {
                        leveledUp = true;
                        rank = "Senior Airman";
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                        tag = "[SrA]";
                    }
                    if (PreMsgs <= 340 && addedmsgs > 340)
                    {
                        leveledUp = true;
                        rank = "Senior Airman II";
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                        tag = "[SrA]";
                    }
                    if (PreMsgs <= 385 && addedmsgs > 385)
                    {
                        leveledUp = true;
                        rank = "Senior Airman III";
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                        tag = "[SrA]";
                    }
                    if (PreMsgs <= 430 && addedmsgs > 430)
                    {
                        leveledUp = true;
                        rank = "Senior Airman IV";
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                        tag = "[SrA]";
                    }
                    if (PreMsgs <= 475 && addedmsgs > 475)
                    {
                        leveledUp = true;
                        rank = "Staff Sergeant";
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                        tag = "[SSgt]";
                    }
                    if (PreMsgs <= 510 && addedmsgs > 510)
                    {
                        leveledUp = true;
                        rank = "Staff Sergeant II";
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                        tag = "[SSgt]";
                    }
                    if (PreMsgs <= 560 && addedmsgs > 560)
                    {
                        leveledUp = true;
                        rank = "Staff Sergeant III";
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                        tag = "[SSgt]";
                    }
                    if (PreMsgs <= 610 && addedmsgs > 610)
                    {
                        leveledUp = true;
                        rank = "Staff Sergeant IV";
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                        tag = "[SSgt]";
                    }
                    if (PreMsgs <= 660 && addedmsgs > 660)
                    {
                        leveledUp = true;
                        rank = "Technical Sergeant";
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                        tag = "[TSgt]";
                    }
                    if (PreMsgs <= 710 && addedmsgs > 710)
                    {
                        leveledUp = true;
                        rank = "Technical Sergeant II";
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                        tag = "[TSgt]";
                    }
                    if (PreMsgs <= 760 && addedmsgs > 760)
                    {
                        leveledUp = true;
                        rank = "Technical Sergeant III";
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                        tag = "[TSgt]";
                    }
                    if (PreMsgs <= 800 && addedmsgs > 800)
                    {
                        leveledUp = true;
                        rank = "Technical Sergeant IV";
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                        tag = "[TSgt]";
                    }
                    if (PreMsgs <= 1000 && addedmsgs > 1000)
                    {
                        leveledUp = true;
                        rank = "Master Sergeant";
                        badgeUrl = "https://i.imgur.com/HhbZ6qe.png";
                        rewards = "**Custom blue PCPI V1 Logo**";
                        tag = "[MSgt]";
                    }
                    if (PreMsgs <= 1250 && addedmsgs > 1250)
                    {
                        leveledUp = true;
                        rank = "First Master Sergeant";
                        badgeUrl = "https://i.imgur.com/4k8FYQE.png";
                        tag = "[MSgt]";
                    }
                    if (PreMsgs <= 1500 && addedmsgs > 1500)
                    {
                        leveledUp = true;
                        rank = "Senior Master Sergeant";
                        badgeUrl = "https://i.imgur.com/FlnxPQd.png";
                        tag = "[SMSgt]";
                    }
                    if (PreMsgs <= 1750 && addedmsgs > 1750)
                    {
                        leveledUp = true;
                        rank = "First Senior Master Sergeant";
                        badgeUrl = "https://i.imgur.com/9ZcJQhB.png";
                        tag = "[SMSgt]";
                    }
                    if (PreMsgs <= 2250 && addedmsgs > 2250)
                    {
                        leveledUp = true;
                        rank = "Chief Master Sergeant";
                        badgeUrl = "https://i.imgur.com/m3rr6uc.png";
                        tag = "[CMSgt]";
                    }
                    if (PreMsgs <= 2750 && addedmsgs > 2750)
                    {
                        leveledUp = true;
                        rank = "First Chief Master Sergeant";
                        badgeUrl = "https://i.imgur.com/Ax4cWCx.png";
                        tag = "[CMSgt]";
                    }
                    if (PreMsgs <= 3250 && addedmsgs > 3250)
                    {
                        leveledUp = true;
                        rank = "Command Chief Master Sergeant";
                        badgeUrl = "https://i.imgur.com/KWIrlz2.png";
                        tag = "[CCM]";
                    }
                    if (PreMsgs <= 3750 && addedmsgs > 3750)
                    {
                        leveledUp = true;
                        rank = "Chief Master Sergeant of the PC Pilots Forces";
                        badgeUrl = "https://i.imgur.com/YrENO8J.png";
                        tag = "[CMSPF]";
                    }
                    if (PreMsgs <= 4500 && addedmsgs > 4500)
                    {
                        var officer = gld.Roles.FirstOrDefault(x => x.Id == 423788910737489920) as SocketRole;
                        leveledUp = true;
                        rank = "2nd Lieutenant";
                        rewards = "**Officer role** added and access to officer chat also receive a **Silver Officer Logo**";
                        badgeUrl = "https://i.imgur.com/zML8JpS.png";
                        tag = "[2ndLT]";
                        await user.AddRoleAsync(officer);
                    }
                    if (PreMsgs <= 5000 && addedmsgs > 5000)
                    {
                        leveledUp = true;
                        rank = "1st Lieutenant";
                        badgeUrl = "https://i.imgur.com/IBW7ITE.png";
                        tag = "[1stLT]";
                    }
                    if (PreMsgs <= 5500 && addedmsgs > 5500)
                    {
                        leveledUp = true;
                        rank = "Captain";
                        badgeUrl = "https://i.imgur.com/kUx1YUx.png";
                        tag = "[Capt]";
                    }
                    if (PreMsgs <= 6500 && addedmsgs > 6500)
                    {
                        leveledUp = true;
                        rank = "Major";
                        badgeUrl = "https://i.imgur.com/P4stq3Y.png";
                        tag = "[Maj]";
                    }
                    if (PreMsgs <= 8000 && addedmsgs > 8000)
                    {
                        leveledUp = true;
                        rank = "Lieutenant Colonel";
                        badgeUrl = "https://i.imgur.com/R7vJBSb.png";
                        tag = "[LtCol]";
                        rewards = "**Gold PCPI Logo** availiable";
                    }
                    if (leveledUp == true)
                    {
                        if (user.Nickname.Contains(tag)) { tag = "None"; }
                        xp = addedmsgs * 12;
                        EmbedBuilder embed = new EmbedBuilder();
                        embed.WithColor(new Color(0, 170, 230));
                        embed.ThumbnailUrl = badgeUrl;
                        embed.Description = $"**Rank:** {rank}\n**Current XP:** {xp}\n**Rewards:** {rewards}\n**Tag Recieved:** {tag}";
                        string currentnickname;
                        var tags = new string[] { "[N/A]", "[Amn]", "[A1C]", "[SrA]", "[SSgt]", "[TSgt]", "[MSgt]", "[SMSgt]", "[CMSgt]", "[CCM]", "[CMSPF]", "[2ndLT]", "[1stLT]", "[Capt]", "[Maj]", "[LtCol]" };
                        if (user.Nickname == null) { currentnickname = user.Username; }
                        else { currentnickname = user.Nickname; }
                        string newnickname = "";
                        if (!(tag == "None"))
                        {
                            var Partition = Partitioner.Create(0, tags.Length); //Create partition in the array                           
                            Parallel.ForEach(Partition, (range, loopState) =>   //In paralell loops through each partition of the array
                            {
                                for (int i = range.Item1; i < range.Item2; i++) { if (currentnickname.Contains(tags[i])) { newnickname = currentnickname.Replace(tags[i], tag); } }
                            });
                            if (newnickname.Length <= 32)
                            {
                                try { await user.ModifyAsync(x => x.Nickname = newnickname); }
                                catch (Discord.Net.HttpException) { Console.WriteLine($"Failed to give {user.Username} the tag ensure that bot is higher role than them"); }
                            }
                            else { await channel.SendMessageAsync($"Unable to add tagg to {user.Mention} due to the nickname being longer than 32 characters long"); }
                        }
                        embed.Title = $"**{user.Nickname} has been promoted!**";
                        await channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); await channel.SendMessageAsync($"Error occured in function LevelUpAsync!"); }
        }
        [Command("tags")]
        public async Task Tags(IGuildUser user = null)
        {
            bool hastxtFile = true;
            try
            {
                if (user == null) { await ReplyAsync("Please mention a user you want to add tags to"); }
                else
                {
                    using (var cont = new XPContext())
                    {
                        if (!cont.Xp.Any(o => o.DiscordId == user.Id.ToString()))
                        {
                            hastxtFile = false;
                        }
                    }
                    if (hastxtFile == false)
                    {
                        await ReplyAsync("User not not found, therefore no tag can be given :{");
                    }
                    else
                    {
                        using (var cont = new XPContext())
                        {
                            var data = await cont.Xp.FirstOrDefaultAsync(x => x.DiscordId == user.Id.ToString());
                            usermsgs = data.XpAmount;
                        }
                        string tag = "";
                        if (usermsgs < 10 || usermsgs > 10) { tag = "[N/A]"; }
                        if (usermsgs > 60) { tag = "[Amn]"; }
                        if (usermsgs > 160) { tag = "[A1C]"; }
                        if (usermsgs > 300) { tag = "[SrA]"; }
                        if (usermsgs > 475) { tag = "[SSgt]"; }
                        if (usermsgs > 660) { tag = "[TSgt]"; }
                        if (usermsgs > 1000) { tag = "[MSgt]"; }
                        if (usermsgs > 1500) { tag = "[SMSgt]"; }
                        if (usermsgs > 2250) { tag = "[CMSgt]"; }
                        if (usermsgs > 3250) { tag = "[CCM]"; }
                        if (usermsgs > 3750) { tag = "[CMSPF]"; }
                        if (usermsgs > 4500) { tag = "[2ndLT]"; }
                        if (usermsgs > 5000) { tag = "[1stLT]"; }
                        if (usermsgs > 5500) { tag = "[Capt]"; }
                        if (usermsgs > 6500) { tag = "[Maj]"; }
                        if (usermsgs > 8000) { tag = "[LtCol]"; }
                        if (!(user.Nickname == null)) { if (user.Nickname.Contains(tag)) { await ReplyAsync($"{user.Mention} has correct tag assigned of: **{tag}**"); return; } }
                        try
                        {
                            string nickname = "";
                            if (user.Nickname == null) { nickname = user.Username; }
                            else { nickname = user.Nickname; }
                            await user.ModifyAsync(x => x.Nickname = $"{tag} {nickname}");
                            await ReplyAsync($"{user.Mention} now has tag of **{tag}**");
                        }
                        catch (Discord.Net.HttpException) { Console.WriteLine($"Failed to give {user.Username} the tag ensure that bot is higher role than them"); }
                    }
                }
            }
            catch (Exception e) { await ReplyAsync(e.ToString()); }
        }
        [Command("leaderboard"), Name("/leaderboard ranks")]
        public async Task RankLeaderboard([Remainder] string ranks = "")
        {
            if (ranks == "ranks" || ranks == "rank")
            {
                try
                {
                    SocketGuild gld = Context.Guild as SocketGuild;         //Declare guild
                    SocketRole Guest = gld.Roles.FirstOrDefault(x => x.Id == 301350337023836161) as SocketRole;   //Guest role
                    SocketRole VIP = gld.Roles.FirstOrDefault(x => x.Id == 301350531148808202) as SocketRole;   //VIP role
                    List<Xp> Leaderboard = new List<Xp> { };      //Declare new empty list
                    using (var cont = new XPContext())
                    {
                        Leaderboard = await cont.Xp.ToListAsync();
                        Leaderboard = Leaderboard.OrderByDescending(xp => xp.XpAmount).Take(15).ToList();
                        Leaderboard.ForEach(x => x.XpAmount *= 12);
                    }
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithColor(new Color(0, 170, 230));
                    embed.ThumbnailUrl = "https://i.imgur.com/A1bgENa.png";
                    //Leaderboard.RemoveRange(Leaderboard.Count / 2, Leaderboard.Count / 2);
                    //Leaderboard.RemoveRange(Leaderboard.Count / 2, Leaderboard.Count / 2);
                    //Leaderboard.RemoveRange(Leaderboard.Count / 2, Leaderboard.Count / 2);
                    string output = "";
                    foreach (var data in Leaderboard)
                    {
                        output = output + $"{Leaderboard.IndexOf(data) + 1}) <@{data.DiscordId}> - {data.XpAmount}xp\n";
                    }
                    embed.Title = "__**XP Leaderboard**__";
                    embed.Description = output;
                    embed.WithFooter("First place is at top working down");
                    await ReplyAsync("", false, embed.Build());
                }
                catch (Exception e) { throw new ArgumentException(e.ToString()); }
            }
        }
        [Command("smashfile")]
        public async Task Delete(SocketGuildUser user = null)
        {
            SocketGuild gld = Context.Guild as SocketGuild;
            SocketGuildUser Author = Context.Message.Author as SocketGuildUser;
            SocketRole mods = gld.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
            SocketRole trialMods = gld.Roles.FirstOrDefault(x => x.Id == 675713190969081866) as SocketRole;
            SocketRole squadLeader = gld.GetRole(311526363196817408) as SocketRole;
            string FileName = $"{directory}{user.Id}.txt";
            if (Author.Roles.Contains(mods) || Author.Roles.Contains(trialMods) || Author.Roles.Contains(squadLeader) || Author.Id == 348505747828506624)
            {
                if (user == null) { await ReplyAsync("Please spesify a user"); return; }
                if (File.Exists(FileName) == false) { await ReplyAsync("No file bish"); return; }
                File.Delete(FileName);
                await ReplyAsync($"{user.Username}'s File Deleted");
            }
        }
        [Command("clear f15 console"), Summary("Clears the log after it gets too full")]
        public async Task ClearLog()
        {
            if (!(Context.User.Id == 348505747828506624)) { await ReplyAsync("Sorry man, small thing called permissions!"); return; }
            Console.Clear();        //clears the console           
            await ReplyAsync("<@348505747828506624> the console has been cleared");   //mwessage output saying the console has been cleared        
        }

        [Command("rank")]
        public async Task Rank(SocketGuildUser user = null)
        {
            SocketGuild gld = Context.Guild as SocketGuild;
            SocketGuildUser Author = Context.User as SocketGuildUser;
            SocketRole YouTubeTeam = gld.Roles.FirstOrDefault(x => x.Id == 301351461424594957) as SocketRole;   //YouTube Team
            SocketRole staff = gld.Roles.FirstOrDefault(x => x.Id == 257984752236101643) as SocketRole;      //Staff
            SocketRole Leader = gld.Roles.FirstOrDefault(x => x.Id == 301350078151393283) as SocketRole; //Leader
            SocketRole Owner = gld.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;  //owner
            SocketRole Member = gld.Roles.FirstOrDefault(x => x.Id == 339703419537457175) as SocketRole;  //member
            SocketRole Recruits = gld.Roles.FirstOrDefault(x => x.Id == 428508411244707841) as SocketRole;
            bool hasFile = false;
            if (user == null) { user = Author; }
            else
            {
                SocketRole mods = gld.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
                SocketRole trialMods = gld.Roles.FirstOrDefault(x => x.Id == 675713190969081866) as SocketRole;
                SocketRole Test = gld.Roles.FirstOrDefault(x => x.Id == 311526363196817408) as SocketRole;
                if (Author.Roles.Contains(mods) || Author.Roles.Contains(Test) || Author.Roles.Contains(Owner) || Author.Roles.Contains(trialMods) || Author.Id == 348505747828506624) { }
                else { await ReplyAsync("Small thing called permissions :3"); return; }

            }
            if (user.Roles.Contains(Member) || user.Roles.Contains(staff) || user.Roles.Contains(Recruits) || user.Roles.Contains(YouTubeTeam) || user.Roles.Contains(Owner) || user.Roles.Contains(Leader))
            {
                if (user.Id == 366986823274201089) { await ReplyAsync($"Oi {user.Mention} you dont have a rank"); return; }
                else if (user.Id == 303593046635708418) { return; }
                using (var cont = new XPContext())
                {
                    if (!cont.Xp.Any(o => o.DiscordId == user.Id.ToString()))
                    {
                        await Insert(user);
                        hasFile = true;
                    }
                    else
                    {
                        hasFile = true;
                    }
                }
                if (hasFile)
                {
                    int currentusersmsgs = 0;
                    using (var cont = new XPContext())
                    {
                        var data = await cont.Xp.FirstOrDefaultAsync(x => x.DiscordId == user.Id.ToString());
                        currentusersmsgs = data.XpAmount;
                    }
                    string userrank = "Airman Basic";
                    int xpremain = 10 - currentusersmsgs;
                    string badgeUrl = "https://i.imgur.com/A1bgENa.png";
                    if (currentusersmsgs >= 10)
                    {
                        userrank = "Airman Basic II";
                        xpremain = 25 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/A1bgENa.png";
                    }
                    if (currentusersmsgs >= 25)
                    {
                        userrank = "Airman Basic III";
                        xpremain = 40 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/A1bgENa.png";
                    }
                    if (currentusersmsgs >= 40)
                    {
                        userrank = "Airman Basic IV";
                        xpremain = 60 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/A1bgENa.png";
                    }
                    if (currentusersmsgs >= 60)
                    {
                        userrank = "Airman";
                        xpremain = 80 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                    }
                    if (currentusersmsgs >= 80)
                    {
                        userrank = "Airman II";
                        xpremain = 105 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                    }
                    if (currentusersmsgs >= 105)
                    {
                        userrank = "Airman III";
                        xpremain = 130 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                    }
                    if (currentusersmsgs >= 130)
                    {
                        userrank = "Airman IV";
                        xpremain = 160 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/m5g65US.png";
                    }
                    if (currentusersmsgs >= 160)
                    {
                        userrank = "Airman 1st Class";
                        xpremain = 190 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                    }
                    if (currentusersmsgs >= 190)
                    {
                        userrank = "Airman 1st Class II";
                        xpremain = 225 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                    }
                    if (currentusersmsgs >= 225)
                    {
                        userrank = "Airman 1st Class III";
                        xpremain = 260 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                    }
                    if (currentusersmsgs >= 260)
                    {
                        userrank = "Airman 1st Class IV";
                        xpremain = 300 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/LAXe36a.png";
                    }
                    if (currentusersmsgs >= 300)
                    {
                        userrank = "Senior Airman";
                        xpremain = 340 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                    }
                    if (currentusersmsgs >= 340)
                    {
                        userrank = "Senior Airman II";
                        xpremain = 385 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                    }
                    if (currentusersmsgs >= 385)
                    {
                        userrank = "Senior Airman III";
                        xpremain = 430 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                    }
                    if (currentusersmsgs >= 430)
                    {
                        userrank = "Senior Airman IV";
                        xpremain = 475 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/o81y9V2.png";
                    }
                    if (currentusersmsgs >= 475)
                    {
                        userrank = "Staff Sergeant";
                        xpremain = 510 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                    }
                    if (currentusersmsgs >= 510)
                    {
                        userrank = "Staff Sergeant II";
                        xpremain = 560 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                    }
                    if (currentusersmsgs >= 560)
                    {
                        userrank = "Staff Sergeant III";
                        xpremain = 610 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                    }
                    if (currentusersmsgs >= 610)
                    {
                        userrank = "Staff Sergeant IV";
                        xpremain = 660 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/0Nfj2kV.png";
                    }
                    if (currentusersmsgs >= 660)
                    {
                        userrank = "Technical Sergeant";
                        xpremain = 710 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                    }
                    if (currentusersmsgs >= 710)
                    {
                        userrank = "Technical Sergeant II";
                        xpremain = 760 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                    }
                    if (currentusersmsgs >= 760)
                    {
                        userrank = "Technical Sergeant III";
                        xpremain = 800 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                    }
                    if (currentusersmsgs >= 800)
                    {
                        userrank = "Technical Sergeant IV";
                        xpremain = 1000 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/avQuA23.png";
                    }
                    if (currentusersmsgs >= 1000)
                    {
                        userrank = "Master Sergeant";
                        xpremain = 1250 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/HhbZ6qe.png";
                    }
                    if (currentusersmsgs >= 1250)
                    {
                        userrank = "First Master Sergeant";
                        xpremain = 1500 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/4k8FYQE.png";
                    }
                    if (currentusersmsgs >= 1500)
                    {
                        userrank = "Senior Master Sergeant";
                        xpremain = 1750 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/FlnxPQd.png";
                    }
                    if (currentusersmsgs >= 1750)
                    {
                        userrank = "First Senior Master Sergeant";
                        xpremain = 2250 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/9ZcJQhB.png";
                    }
                    if (currentusersmsgs >= 2250)
                    {
                        userrank = "Chief Master Sergeant";
                        xpremain = 2750 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/m3rr6uc.png";
                    }
                    if (currentusersmsgs >= 2750)
                    {
                        userrank = "First Chief Master Sergeant";
                        xpremain = 3250 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/Ax4cWCx.png";
                    }
                    if (currentusersmsgs >= 3250)
                    {
                        userrank = "Command Chief Master Sergeant";
                        xpremain = 3750 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/KWIrlz2.png";
                    }
                    if (currentusersmsgs >= 3750)
                    {
                        userrank = "Chief Master Sergeant of the PC Pilots Forces";
                        xpremain = 4500 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/YrENO8J.png";
                    }
                    if (currentusersmsgs >= 4500)
                    {
                        userrank = "2nd Lieutenant";
                        xpremain = 5000 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/zML8JpS.png";
                    }
                    if (currentusersmsgs >= 5000)
                    {
                        userrank = "1st Lieutenant";
                        xpremain = 5500 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/IBW7ITE.png";
                    }
                    if (currentusersmsgs >= 5500)
                    {
                        userrank = "Captain";
                        xpremain = 6500 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/kUx1YUx.png";
                    }
                    if (currentusersmsgs >= 6500)
                    {
                        userrank = "Major";
                        xpremain = 8000 - currentusersmsgs;
                        badgeUrl = "https://i.imgur.com/P4stq3Y.png";
                    }
                    if (currentusersmsgs >= 8000)
                    {
                        userrank = "Lieutenant Colonel";
                        xpremain = 0;
                        badgeUrl = "https://i.imgur.com/R7vJBSb.png";
                    }
                    if (user.Roles.Contains(YouTubeTeam))
                    {
                        userrank = "Colonel";
                        xpremain = 0;
                        badgeUrl = "https://i.imgur.com/4o8pqVS.png";
                    }
                    if (user.Id == 201468045116440576) //Surl
                    {
                        userrank = "General";
                        xpremain = 0;
                        badgeUrl = "https://i2.wp.com/pcpilotscrew.com/wp-content/uploads/2018/03/PCPI_Ranks_V2_23-150x150.png?resize=50%2C50";
                    }
                    if (user.Id == 154609310188437504)   //Deathshots
                    {
                        userrank = "Colonel";
                        xpremain = 0;
                        badgeUrl = "https://i.imgur.com/4o8pqVS.png";
                    }
                    if (user.Id == 129531257452363776)  //Ajay
                    {
                        userrank = "Lieutenant General";
                        xpremain = 0;
                        badgeUrl = "https://i.imgur.com/ACe5e3j.png";
                    }
                    if (user.Id == 175673821171286016)   //Liinex
                    {
                        userrank = "Brigadier General";
                        xpremain = 0;
                        badgeUrl = "https://i.imgur.com/rRW7wJ0.png";
                    }
                    if (user.Roles.Contains(Owner))   //H8
                    {
                        userrank = "General of the PC Pilots Forces";
                        xpremain = 0;
                        badgeUrl = "https://i.imgur.com/KpCzePz.png";
                    }
                    xpremain = xpremain * 12;
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.Title = $"__{user.Nickname}__";
                    embed.ThumbnailUrl = badgeUrl;
                    embed.WithColor(new Color(0, 170, 230))
                    .AddField(y =>
                    {
                        y.Name = "Your XP:";
                        y.Value = $"**{currentusersmsgs * 12}**";
                        y.IsInline = true;
                    })
                    .AddField(y =>
                    {
                        y.Name = "Your Rank:";
                        y.Value = $"**{userrank}**";
                        y.IsInline = true;
                    })
                    .AddField(y =>
                    {
                        y.Name = "Next Rank:";
                        y.Value = $"**{xpremain}**";
                        y.IsInline = false;
                    })
                    .AddField(y =>
                    {
                        y.Name = "Squadron Rank(s):";
                        y.Value = squadRank(user, gld);
                        y.IsInline = true;
                    });
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else { await ReplyAsync("You have to be a member of our crew to rank up. Visit https://pcpilotscrew.com/join-our-crew for more info."); }
        }
        private string squadRank(SocketGuildUser user, SocketGuild gld)
        {
            string rank = "";
            List<ulong> Roles = new List<ulong>();
            bool GTA_Elite = false, GTA_Expert = false, GTA_Novice = false, WT_Elite = false, WT_Expert = false, WT_Novice = false, DCS_Elite = false, DCS_Expert = false, DCS_Novice = false;

            foreach (SocketRole a in user.Roles)
            {
                Roles.Add(a.Id);
            }

            if (Roles.Contains(339703419537457175)) //Member
            {
                if (Roles.Contains(339708449174585345)) //Start GTA
                {
                    GTA_Elite = true;
                }
                else
                {
                    if (Roles.Contains(680033628058943499))
                    {
                        GTA_Expert = true;
                    }
                    else
                    {
                        if (Roles.Contains(257984933702795264))
                        {
                            GTA_Novice = true;
                        }
                    }
                }           //End GTA

                if (Roles.Contains(687431534961098862)) //Start WT
                {
                    WT_Elite = true;
                }
                else
                {
                    if (Roles.Contains(687431082932568156))
                    {
                        WT_Expert = true;
                    }
                    else
                    {
                        if (Roles.Contains(687581309052387418))
                        {
                            WT_Novice = true;
                        }
                    }
                }           //End WT

                if (Roles.Contains(687431876188700792)) //Start DCS
                {
                    DCS_Elite = true;
                }
                else
                {
                    if (Roles.Contains(687431894010298411))
                    {
                        DCS_Expert = true;
                    }
                    else
                    {
                        if (Roles.Contains(687581314945253416))
                        {
                            DCS_Novice = true;
                        }
                    }
                }           //End DCS

                ///////////////////

                if (Roles.Contains(339708449174585345) || Roles.Contains(680033628058943499) || Roles.Contains(257984933702795264) == true) //GTA
                {
                    if (Roles.Contains(687431534961098862) || Roles.Contains(687431082932568156) || Roles.Contains(687581309052387418) == true) //WT
                    {
                        if (Roles.Contains(687431876188700792) || Roles.Contains(687431894010298411) || Roles.Contains(687581314945253416) == true) //DCS
                        {
                            goto Write_DCS;
                        }
                        else
                        {
                            goto Write_WT;
                        }
                    }
                    else
                    {
                        goto Write_GTAV;
                    }
                }
                else
                {
                    if (Roles.Contains(426745457662885898) || Roles.Contains(676060833150926858) == false)
                    {
                        goto Write_Member;
                    }
                    else
                    {
                        goto Write_WT;
                    }
                }
            }




        Write_GTAV:
            if (Roles.Contains(426745071967141909)) //GTAV Role
            {
                if (GTA_Elite == true)
                {
                    rank = rank + "\n **GTA Elite**";
                    goto Write_WT;
                }
                else
                {
                    if (GTA_Expert == true)
                    {
                        rank = rank + "\n **GTA Expert**";
                        goto Write_WT;
                    }
                    else
                    {
                        if (GTA_Novice == true)
                        {
                            rank = rank + "\n **GTA Novice**";
                            goto Write_WT;
                        }
                        else
                        {
                            goto Write_WT;
                        }
                    }
                }
            }
            else
            {
                goto Write_WT;
            }

        Write_WT:
            if (Roles.Contains(426745457662885898)) //WT Role
            {
                if (WT_Elite == true)
                {
                    rank = rank + "\n **War Thunder Elite**";
                    goto Write_DCS;
                }
                else
                {
                    if (WT_Expert == true)
                    {
                        rank = rank + "\n **War Thunder Expert**";
                        goto Write_DCS;
                    }
                    else
                    {
                        if (WT_Novice == true)
                        {
                            rank = rank + "\n **War Thunder Novice**";
                            goto Write_DCS;
                        }
                        else
                        {
                            goto Write_DCS;
                        }
                    }
                }
            }
            else
            {
                goto Write_DCS;
            }

        Write_DCS:
            if (Roles.Contains(676060833150926858)) //DCS Role
            {
                if (DCS_Elite == true)
                {
                    rank = rank + "\n **DCS Elite**";
                    goto Return;
                }
                else
                {
                    if (DCS_Expert == true)
                    {
                        rank = rank + "\n **DCS Expert**";
                        goto Return;
                    }
                    else
                    {
                        if (DCS_Novice == true)
                        {
                            rank = rank + "\n **DCS Novice**";
                            goto Return;
                        }
                        else
                        {
                            goto Write_Member;
                        }
                    }
                }
            }
            else
            {
                if (Roles.Contains(426745071967141909) || Roles.Contains(426745457662885898))
                {
                    goto Return;
                }
                else
                {
                    rank = rank + "\n **Newcomer**";
                }
            }

        Write_Member:
            if (GTA_Elite || GTA_Expert || GTA_Novice || WT_Elite || WT_Expert || WT_Novice || DCS_Elite || DCS_Expert || DCS_Novice == true)
            {
                goto Return;
            }
            if (Roles.Contains(426745071967141909)) //GTAV
            {
                if (Roles.Contains(426745457662885898)) //WT
                {
                    if (Roles.Contains(676060833150926858)) //DCS
                    {
                        rank = rank + "\n **Newcomer GTAV / WT / DCS**";
                    }
                    else
                    {
                        rank = rank + "\n **Newcomer GTAV & WT**";
                    }
                }
                else
                {
                    if (Roles.Contains(676060833150926858)) //DCS
                    {
                        rank = rank + "\n **Newcomer GTAV & DCS**";
                    }
                    else
                    {
                        rank = rank + "\n **Newcomer GTAV**";
                    }
                }
            }
            else
            {
                if (Roles.Contains(426745457662885898)) //WT
                {
                    if (Roles.Contains(676060833150926858)) //DCS
                    {
                        rank = rank + "\n **Newcomer WT & DCS**";
                    }
                    else
                    {
                        rank = rank + "\n **Newcomer WT**";
                    }
                }
                else
                {
                    if (Roles.Contains(676060833150926858)) //DCS
                    {
                        rank = rank + "\n **Newcomer DCS**";
                    }
                    else
                    {
                        rank = rank + "\n **Newcomer**";
                    }
                }
            }

        Return:
            if (rank == "")
            {
                rank = "Humm ?";
            }
            return rank;
        }
        
        public async Task Debug()
        {
            using (Process process = Process.GetCurrentProcess())
            {
                var embed = new EmbedBuilder();
                SocketGuild gld = Context.Guild as SocketGuild;
                IGuildUser Buzzard = gld.GetUser(303593046635708418) as IGuildUser;
                var application = await Context.Client.GetApplicationInfoAsync(); /*for lib version*/
                embed.ThumbnailUrl = Buzzard.GetAvatarUrl();
                embed.WithColor(new Discord.Color(0, 170, 230))
                .AddField(y =>
                {
                    /*new embed field*/
                    y.Name = "Author.";                            /*Field name here*/
                    y.Value = "ElitistStone";     /*Code here. If INT convert to string*/
                    y.IsInline = true;
                })
                .AddField(y =>                           /* add new field, rinse and repeat*/
                {
                    y.Name = "Uptime.";
                    var time = DateTime.Now - process.StartTime;         /* Subtracts current time and start time to get Uptime*/
                    var sb = new StringBuilder();
                    if (time.Days > 0) { sb.Append($"{time.Days}d "); }
                    if (time.Hours > 0) { sb.Append($"{time.Hours}h "); }
                    if (time.Minutes > 0) { sb.Append($"{time.Minutes}m "); }
                    sb.Append($"{time.Seconds}s ");
                    y.Value = sb.ToString();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Discord.net version.";            /*pulls discord lib version*/
                    y.Value = DiscordConfig.Version;
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Version Number";
                    y.Value = "2.5.7";
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Number Of Users";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count).ToString();        /*Counts users*/
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Channels";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count).ToString();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Priority Class";
                    y.Value = process.PriorityClass;
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Physical Memory Allocated";
                    y.Value = $"{process.WorkingSet64 / 1000000}MB";
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Memory Usage";
                    y.Value = $"{process.PrivateMemorySize64 / 1000000}MB";
                    y.IsInline = true;
                });
                embed.WithFooter($"{DateTime.Now} UK Time");                                         //the footer of the embed which is the remaining messages until they level up
                await ReplyAsync("", embed: embed.Build());
            }
        }
    }
}
