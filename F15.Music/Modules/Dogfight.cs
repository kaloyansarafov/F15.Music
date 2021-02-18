using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;

namespace F15.Music.Modules
{
    public class Dogfight : ModuleBase
    {
        private static string SwitchCaseString = "nofight";   //declaring variables 
        private static IUser player1;
        private static IUser player2;
        private static string whosTurn;
        private static string whoWaits;
        private static string placeHolder;
        private static int health1 = 100;
        private static int health2 = 100;
        private static int block1 = 6;
        private static int block2 = 6;
        private static int canBlock1 = 0;
        private static int canBlock2 = 0;
        private static int dogfight = 0;
        private static int CanStall1 = 1;
        private static int CanStall2 = 1;
        private static int PrevDamage1 = 0;
        private static int PrevDamage2 = 0;
        [Command("dogfight"), Summary("starts a fight with the @Mention user (example: !fight Link5012"), Alias("Fight")]
        public async Task Fight(IUser user = null)
        {
            var gld = Context.Guild as SocketGuild;
            if (gld.Users.Contains(user))
            {
                if (!(Context.Channel.Id == 301368047820865546)) { await ReplyAsync("Dogfights can only be started in <#301368047820865546>"); return; }
                if (user.Status.ToString().ToLower() == "offline") { await ReplyAsync("User challenged is not online!"); return; }
                if (user.Id == 366986823274201089)
                {
                    await ReplyAsync("/kill");
                    await ReplyAsync(user.Mention + " you hit and did 1000 damage! ⚔️\n\n" + Context.User.Mention + " died. **BOOM** " + user.Mention + " won!");
                    SwitchCaseString = "nofight";
                    health1 = 100;
                    health2 = 100;
                    block1 = 5;
                    block2 = 5;
                    canBlock2 = 0;
                    canBlock1 = 0;
                    dogfight = 0;
                    CanStall1 = 1;
                    CanStall2 = 1;
                    PrevDamage1 = 0;
                    PrevDamage2 = 0;
                    return;
                }
                if (Context.User.Mention != user.Mention && SwitchCaseString == "nofight")
                {
                    if (user == null) { await ReplyAsync("Who do you want to dogfight?"); return; }
                    SwitchCaseString = "fight_p1";
                    player1 = Context.User;
                    player2 = user;
                    string[] whoStarts = new string[] { Context.User.Mention, user.Mention };
                    Random rand = new Random();
                    int randomIndex = rand.Next(whoStarts.Length);
                    string text = whoStarts[randomIndex];
                    whosTurn = text;
                    dogfight = 1;
                    if (text == Context.User.Mention) { whoWaits = user.Mention; }
                    else { whoWaits = Context.User.Mention; }
                    await ReplyAsync("Dogfight has started between " + Context.User.Mention + " and " + user.Mention + "!\n\n" + player1.Mention + " you got " + health1 + " health!\n" + player2.Mention + " you got " + health2 + " health!\n\n" + text + " your turn!");
                }
                else { await ReplyAsync(Context.User.Mention + " sorry but there is a fight going on right now, or you just tried to fight urself..."); }
            }
            else { await ReplyAsync("Pretty sure this user isnt in the server!"); }
        }
        [Command("kill")]
        public async Task Kill(string input = "")
        {
            if (input == "" || input != "")
            {
                if (Context.User.Id == 366986823274201089) { return; }
                else if (SwitchCaseString == "fight_p1") { await ReplyAsync("Nah M8"); }
                else { await ReplyAsync("No dogfight at the moment"); }
            }
        }
        [Command("giveup"), Summary("Stops the fight and gives up."), Alias("GiveUp", "Giveup", "giveUp")]
        public async Task GiveUp(string input = "")
        {
            if (input == "" || input != "")
            {
                if (!(Context.Channel.Id == 301368047820865546)) { await ReplyAsync("Dogfight commands can only be used in <#301368047820865546>"); return; }
                SocketGuild gld = Context.Guild as SocketGuild;
                SocketRole mods = gld.Roles.FirstOrDefault(x => x.Id == 311889967679012864) as SocketRole;
                SocketRole Owner = gld.Roles.FirstOrDefault(x => x.Id == 257984497071554560) as SocketRole;
                SocketGuildUser user = Context.User as SocketGuildUser;
                if (dogfight == 0) { await ReplyAsync("There is no dogfight to give up on :3"); return; }
                if (user.Id == player1.Id || user.Id == player2.Id || user.Roles.Contains(mods) || user.Roles.Contains(Owner))
                {
                    if (SwitchCaseString == "fight_p1")
                    {
                        await ReplyAsync("The dogfight stopped.");
                        ResetGame();
                    }
                    else { await ReplyAsync("There is no fight to stop."); }
                }
                else { await ReplyAsync("You cannot end the dogfight"); }
            }
            else { await ReplyAsync("No need to say stuff after this command :3"); }
        }
        [Command("shoot"), Summary("Slashes your foe with a sword. Good accuracy and medium damage"), Alias("slash")]
        public async Task Slash([Remainder] string input = "")
        {
            if (input == "" || input != "")
            {
                if (!(Context.Channel.Id == 301368047820865546)) { await ReplyAsync("Dogfight commands can only be used in <#301368047820865546>"); return; }
                if (SwitchCaseString == "fight_p1")
                {
                    if (whosTurn == Context.User.Mention)
                    {
                        Random rand2 = new Random();
                        int randomIndex2 = rand2.Next(7, 15);
                        if (Context.User.Id != player1.Id)
                        {
                            health1 = health1 - randomIndex2;
                            if (health1 > 0)
                            {
                                placeHolder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeHolder;
                                await ReplyAsync(Context.User.Mention + " you hit and did " + randomIndex2 + " damage! ⚔️\n\n" + player1.Mention + " got " + health1 + " health left!\n" + player2.Mention + " got " + health2 + " health left!\n\n" + whosTurn + " your turn!");
                                canBlock2 = 1;
                                PrevDamage1 = randomIndex2;
                            }
                            else
                            {
                                await ReplyAsync(Context.User.Mention + " you hit and did " + randomIndex2 + " damage! ⚔️\n\n" + player1.Mention + " died. **BOOM** " + player2.Mention + " won!");
                                ResetGame();
                            }
                        }
                        else if (Context.User.Id == player1.Id)
                        {
                            health2 = health2 - randomIndex2;
                            if (health2 > 0)
                            {
                                placeHolder = whosTurn;
                                whosTurn = whoWaits;
                                whoWaits = placeHolder;
                                await ReplyAsync(Context.User.Mention + " you hit and did " + randomIndex2 + " damage! ⚔️\n\n" + player1.Mention + " got " + health1 + " health left!\n" + player2.Mention + " got " + health2 + " health left!\n\n" + whosTurn + " your turn!");
                                canBlock1 = 1;
                                PrevDamage2 = randomIndex2;
                            }
                            else
                            {
                                await ReplyAsync(Context.User.Mention + " you hit and did " + randomIndex2 + " damage! ⚔️\n\n" + player2.Mention + " died. **BOOM**" + player1.Mention + " won!");
                                ResetGame();
                            }
                        }
                        else { await ReplyAsync("Sorry it seems like something went wrong. Pls type /giveup :c"); }
                    }
                    else { await ReplyAsync(Context.User.Mention + " Hold on, hold on. It isn't your turn :3"); }
                }
                else { await ReplyAsync("There is no dogfight at the moment. Sorry :/ You can challenge someone by typing /dogfight [@user]"); }
            }
            else { await Context.Channel.SendMessageAsync("You just need to type /shoot, no spesified user is needed "); }
        }
        [Command("stall")]
        public async Task Stall([Remainder] string input = "")
        {
            if (input == "" || input != "")
            {
                if (!(Context.Channel.Id == 301368047820865546)) { await ReplyAsync("Dogfight commands can only be used in <#301368047820865546>"); return; }
                if (SwitchCaseString == "fight_p1")
                {
                    if (whosTurn == Context.User.Mention)
                    {
                        Random rand2 = new Random();
                        int randomIndex2 = rand2.Next(15, 20);
                        Random rand1 = new Random();
                        int randomstall = rand1.Next(1, 6);
                        if (Context.User.Id != player1.Id)
                        {
                            if (CanStall1 == 0) { await ReplyAsync("You cant stall at the moment"); }
                            else
                            {
                                if (randomstall != 1)
                                {
                                    placeHolder = whosTurn;
                                    whosTurn = whoWaits;
                                    whoWaits = placeHolder;
                                    CanStall2 = 1;
                                    CanStall1 = 1;
                                    await ReplyAsync($"{Context.User.Mention} your stall was unsuccessful! {whosTurn} its now your turn");
                                }
                                else
                                {
                                    health1 = health1 - randomIndex2;
                                    if (health1 > 0)
                                    {
                                        placeHolder = whosTurn;
                                        whosTurn = whoWaits;
                                        whoWaits = placeHolder;
                                        CanStall2 = 1;
                                        CanStall1 = 1;
                                        await ReplyAsync(Context.User.Mention + " stalled and did  " + randomIndex2 + " damage\n\n" + whosTurn + " you have " + health1 + " health \n\n" + whosTurn + " its now your turn");
                                    }
                                    else
                                    {
                                        await ReplyAsync(Context.User.Mention + " you hit and did " + randomIndex2 + " damage! ⚔️\n\n" + player1.Mention + " died. **BOOM** " + player2.Mention + " won!");
                                        ResetGame();
                                    }
                                }
                            }
                        }
                        else if (Context.User.Id == player1.Id)
                        {
                            if (CanStall2 == 0) { await ReplyAsync("You cant stall at the moment"); }
                            else
                            {
                                if (randomstall != 1)
                                {
                                    placeHolder = whosTurn;
                                    whosTurn = whoWaits;
                                    whoWaits = placeHolder;
                                    CanStall2 = 1;
                                    CanStall1 = 1;
                                    await ReplyAsync($"{Context.User.Mention} your stall was unsuccessful! {whosTurn} its now your turn");
                                }
                                else
                                {
                                    health2 = health2 - randomIndex2;
                                    if (health2 > 0)
                                    {
                                        placeHolder = whosTurn;
                                        whosTurn = whoWaits;
                                        whoWaits = placeHolder;
                                        CanStall1 = 1;
                                        CanStall2 = 1;
                                        await ReplyAsync(Context.User.Mention + " stalled and did  " + randomIndex2 + " damage\n\n" + whosTurn + " you have " + health2 + " health \n\n" + whosTurn + " its now your turn");
                                    }
                                    else
                                    {
                                        await ReplyAsync(Context.User.Mention + " you hit and did " + randomIndex2 + " damage! ⚔️\n\n" + player1.Mention + " died. **BOOM** " + player2.Mention + " won!");
                                        ResetGame();
                                    }
                                }
                            }
                        }
                        else { await ReplyAsync("Sorry it seems like something went wrong. Pls type /giveup"); }
                    }
                    else { await ReplyAsync(Context.User.Mention + " it is not your turn."); }
                }
                else { await ReplyAsync("There is no dogfight at the moment. Sorry :/ You can challenge someone by typing /dogfight [@user]"); }
            }
        }
        [Command("dodge"), Summary("block opponents attack"), Alias("block")]
        public async Task Heavy([Remainder] string input = "")
        {
            if (input == "" || input != "")
            {
                if (!(Context.Channel.Id == 301368047820865546)) { await ReplyAsync("Dogfight commands can only be used in <#435797624193417216>"); return; }
                if (SwitchCaseString == "fight_p1")
                {
                    if (whosTurn == Context.User.Mention)
                    {
                        Random rand2 = new Random();
                        int randomIndex2 = rand2.Next(7, 10);
                        if (Context.User.Id != player1.Id)
                        {
                            if (block1 < 1)
                            {
                                var replies = new string[] { "You have exhausted your dodging moves ", "You can no longer dodge, oh dear :smirk: " };  //declares array for different replies 
                                string reply = replies[new Random().Next(0, replies.Length)];
                                await ReplyAsync(reply);
                            }
                            else
                            {
                                if (canBlock1 == 0)
                                {
                                    var replies = new string[] { "You cant block at the moment ", "You cnt dodge when someone dodged jeez :smirk: " };  //declares array for different replies 
                                    string reply = replies[new Random().Next(0, replies.Length)];
                                    await ReplyAsync(reply);
                                }
                                else
                                {
                                    health2 = health2 + PrevDamage2;
                                    block1 = block1 - 1;
                                    placeHolder = whosTurn;
                                    whosTurn = whoWaits;
                                    whoWaits = placeHolder;
                                    canBlock2 = 0;
                                    await ReplyAsync(Context.User.Mention + " dodged the previous attack. \n\n" + Context.User.Mention + " you have " + health2 + " health \n\n" + whosTurn + " its now your turn");
                                }
                            }
                        }
                        else if (Context.User.Id == player1.Id)
                        {
                            if (block2 < 1)
                            {
                                var replies = new string[] { "You have exhausted your dodging moves  ", "You can no longer dodge, oh dear :smirk: " };  //declares array for different replies 
                                string reply = replies[new Random().Next(0, replies.Length)];
                                await ReplyAsync(reply);
                            }
                            else
                            {
                                if (canBlock2 == 0)
                                {
                                    var replies = new string[] { "You can not dodge right now  ", "You can not dodge after someone else dodged :smirk: " };  //declares array for different replies 
                                    string reply = replies[new Random().Next(0, replies.Length)];
                                    await ReplyAsync(reply);
                                }
                                else
                                {
                                    health1 = health1 + PrevDamage1;
                                    block2 = block2 - 1;
                                    placeHolder = whosTurn;
                                    whosTurn = whoWaits;
                                    whoWaits = placeHolder;
                                    canBlock1 = 0;
                                    await ReplyAsync(Context.User.Mention + " dodged the previous attack.\n\n" + Context.User.Mention + " you have " + health1 + " health \n\n" + whosTurn + " its now your turn");
                                }
                            }
                        }
                        else { await ReplyAsync("Sorry it seems like something went wrong. Pls type /giveup"); }
                    }
                    else { await ReplyAsync(Context.User.Mention + " it is not your turn."); }
                }
                else { await ReplyAsync("There is no dogfight at the moment. Sorry :/ You can challenge someone by typing /dogfight [@user]"); }
            }
        }
        public void ResetGame()
        {
            SwitchCaseString = "nofight";
            health1 = 100;
            health2 = 100;
            block1 = 4;
            block2 = 4;
            canBlock2 = 0;
            canBlock1 = 0;
            CanStall1 = 1;
            CanStall2 = 1;
            PrevDamage2 = 0;
            PrevDamage1 = 0;
        }
    }
}
