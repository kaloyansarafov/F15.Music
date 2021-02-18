using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using F15.Services;
using Discord;

namespace F15.Music.Modules
{
    public class HangarModule : ModuleBase
    {

        [Command("id")]
        private async Task HangarName([Remainder] string name = "")
        {
            if (Context.Channel.Id == 359339709824237568)
            {
                    if (name != "")
                    {
                        try
                        {
                            int index = isInUserList(Context.User.Id);
                            if (index != -1)
                            {
                                if (isInArray(CommandHandler.hangarUsers[index], 428508411244707841) == true)    //Contains crew recruits ID
                                    await (Context.User as SocketGuildUser).ModifyAsync(x => x.Nickname = $"[N/A] {name}");
                            }
                            else
                            {
                                await (Context.User as SocketGuildUser).ModifyAsync(x => x.Nickname = name);
                                AddNewUserToArray(Context.User.Id);
                            }
                            await ReplyAsync("Nickname saved");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            Console.WriteLine($"Error occured when giving hangar user name");
                            await ReplyAsync("An error has occured please mention the moderators");
                        }
                    }
                else { await ReplyAsync("Please enter a name after the command. Example /id ElitistStone"); }
            }
        }
        private bool isInArray(List<ulong> list, ulong id)
        {
            foreach (ulong item in list)
            {
                if (item == id) { return true; }
            }
            return false;
        }
        public static void AddNewUserToArray(ulong userID)
        {
            List<ulong> userList = new List<ulong> {
                userID
            };
            CommandHandler.hangarUsers.Add(userList);
        }
        public static int isInUserList(ulong userId)
        {
            for (int i = 0; i < CommandHandler.hangarUsers.Count; i++)
            {
                if (CommandHandler.hangarUsers[i].Contains(userId))
                    return i;
            }
            return -1;
        }

        public static async Task HangarDeleteMessages(SocketTextChannel channel)
        {
            var messages = (await channel.GetMessagesAsync().FlattenAsync());
            foreach (IMessage message in messages)
            {
                if (message.Author.Id != 213660637274832897)  //H8's ID
                    await message.DeleteAsync();
            }
        }
    }
}
