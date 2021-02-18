using Discord;
using Discord.WebSocket;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace F15.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly LavaNode _lavaNode;

        public LavaLinkAudio(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
            }

            if (voiceState.VoiceChannel is null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query)
        {
            if (user.VoiceChannel == null)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
            }

            if (!_lavaNode.HasPlayer(guild))
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");
            }

            try
            {
                var player = _lavaNode.GetPlayer(guild);

                LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                }

                track = search.Tracks.FirstOrDefault();

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    Log.Information("Music", $"{track.Title} has been added to the music queue.");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue);
                }

                await player.PlayAsync(track);
                Log.Information("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue);
            }

            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
            }

        }

        public async Task<Embed> LeaveAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                Log.Information("Music", $"Bot has left.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing moosik.", Color.Blue);
            }
            catch (InvalidOperationException ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
            }
        }

        public async Task<Embed> ListAsync(IGuild guild)
        {
            try
            {
                var descriptionBuilder = new StringBuilder();

                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check /Help for info on how to use the bot.");

                if (player.PlayerState is PlayerState.Playing)
                {
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.", Color.Blue);
                    }
                    else
                    {
                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
                            trackNum++;
                        }
                        return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
                    }
                }
                else
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error, Please Contact Draxis.");
                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }

        }

        public async Task<Embed> SkipTrackAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check /Help for info on how to use the bot.");
                if (player.Queue.Count < 1)
                {
                    return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing." +
                        $"\n\nDid you mean /Stop?");
                }
                else
                {
                    try
                    {
                        var currentTrack = player.Track;
                        await player.SkipAsync();
                        Log.Information("Music", $"Bot skipped: {currentTrack.Title}");
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
            }
        }

        public async Task<Embed> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check /Help for info on how to use the bot.");

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                Log.Information("Music", $"Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
        }

        public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        {
            if (volume > 150 || volume <= 0)
            {
                return $"Volume must be between 1 and 150.";
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                Log.Information("Music", $"Bot Volume set to: {volume}");
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}, what a bamboozle.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                //await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue));
        }
    }
}
