using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Victoria;
using F15;
using Buzzard_V2;

namespace F15.Services
{
    public class LoggingService
    {

        private readonly ILogger _logger;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly LavaNode _instanceOfLavaNode;
        private LavaLinkAudio _audioService;

        public LoggingService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _logger = services.GetRequiredService<ILogger<LoggingService>>();
            _instanceOfLavaNode = services.GetRequiredService<LavaNode>();
            _audioService = services.GetRequiredService<LavaLinkAudio>();

            _discord.Ready += OnReadyAsync;
            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
            _instanceOfLavaNode.OnTrackEnded += _audioService.TrackEnded;
        }

        public Task OnReadyAsync()
        {
            if (!_instanceOfLavaNode.IsConnected)
            {
                _instanceOfLavaNode.ConnectAsync();
                _logger.LogInformation("Connected to LavaLink");
            }
            _logger.LogInformation($"Connected as -> [{_discord.CurrentUser}] :)");
            _logger.LogInformation($"We are on [{_discord.Guilds.Count}] servers");
            return Task.CompletedTask;
        }

        public Task OnLogAsync(LogMessage msg)
        {
            string logText = $"{msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            switch (msg.Severity.ToString())
            {
                case "Critical":
                    {
                        _logger.LogCritical(logText);
                        break;
                    }
                case "Warning":
                    {
                        _logger.LogWarning(logText);
                        break;
                    }
                case "Info":
                    {
                        _logger.LogInformation(logText);
                        break;
                    }
                case "Verbose":
                    {
                        _logger.LogInformation(logText);
                        break;
                    }
                case "Debug":
                    {
                        _logger.LogDebug(logText);
                        break;
                    }
                case "Error":
                    {
                        _logger.LogError(logText);
                        break;
                    }
            }

            return Task.CompletedTask;

        }
    }
}