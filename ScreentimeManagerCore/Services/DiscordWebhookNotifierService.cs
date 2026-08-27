using CSharpDiscordWebhook;
using CSharpDiscordWebhook.Objects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreentimeManagerCore.Configuration;
using ScreentimeManagerCore.Interfaces;
using ScreentimeManagerCore.Models;
using System.Drawing;
using System.Globalization;

namespace ScreentimeManagerCore.Services
{
    public class DiscordWebhookNotifierService : IWebhookNotifier
    {
        const string TIMESPAN_FORMAT = @"hh'h 'mm'min 'ss's'";
        const string DISPLAYED_USER = "Screentime Manager";
        const string THUMBNAIL_URL = "https://cdn-icons-png.flaticon.com/512/11320/11320024.png";
        const string ACCOUNT_IMAGE_URL = "https://cdn.pixabay.com/photo/2021/09/20/22/15/hourglass-6641967_1280.png";

        protected readonly ILogger<DiscordWebhookNotifierService> _logger;
        protected readonly IOptions<WebhookNotifierConfiguration> _config;
        protected Uri? _webhookUrl;
        protected DiscordWebhook? _webhook;
        protected CultureInfo _culture = CultureInfo.InvariantCulture;

        public DiscordWebhookNotifierService(ILogger<DiscordWebhookNotifierService> logger, IOptions<WebhookNotifierConfiguration> config)
        {
            _logger = logger;
            _config = config;

            _webhookUrl = new Uri(_config.Value.WebhookUrl != null ? _config.Value.WebhookUrl : "https://localhost");

            _webhook = new DiscordWebhook(_webhookUrl);

            try
            {
                _culture = new CultureInfo(_config.Value.Culture);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Configured culture could not be used.");
                _culture = CultureInfo.InvariantCulture;
            }
        }

        public async Task NotifyHostHasShutdownAsync(HostStatus status)
        {
            await SendNotification(GetHostShutdownMessage, status);
        }

        public async Task NotifyTimeUpAsync(HostStatus status)
        {
            await SendNotification(GetScreentimeUsedMsg, status);
        }

        public async Task NotifyTurnedOnWithoutTimeLeftAsync(HostStatus status)
        {
            await SendNotification(GetHostRestartedWithoutTimeMsg, status);
        }

        protected async Task SendNotification(Func<HostStatus, MessageBuilder> messageGenerator, HostStatus status)
        {
            try
            {
                await _webhook.SendMessageAsync(messageGenerator(status));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification via Discord webhook.");
            }
        }

        protected virtual MessageBuilder GetHostShutdownMessage(HostStatus status)
        {
            var today = DateTime.Today;
            TimeSpan exceeded = status.ScreentimeLeft.Duration();

            return new MessageBuilder
            {
                Content = $"@everyone",
                Username = DISPLAYED_USER,
                AvatarUrl = ACCOUNT_IMAGE_URL,
                Embeds = [
                        new EmbedBuilder
                        {
                                Title = status.Hostname,
                                Description = $"System was shut down.",
                                Color = Color.Green,
                                Thumbnail = new EmbedMediaBuilder { Url = THUMBNAIL_URL },
                                Fields = [
                                    new EmbedFieldBuilder {
                                        Name = "Allowed Time",
                                        Value = $"{status.ScreentimeLimit.ToString(TIMESPAN_FORMAT)}",
                                        Inline = true
                                    },
                                    new EmbedFieldBuilder {
                                        Name = "Exceeded",
                                        Value = $"{status.ScreentimeLeft.Duration().ToString(TIMESPAN_FORMAT)}",
                                        Inline = true
                                    }
                                ],
                                Footer = new EmbedFooterBuilder { Text = DISPLAYED_USER},
                                Timestamp = DateTime.Now
                        }
                ]
            };
        }

        protected virtual MessageBuilder GetScreentimeUsedMsg(HostStatus status)
        {
            var today = DateTime.Today;
            return new MessageBuilder
            {
                Content = $"@everyone",
                Username = DISPLAYED_USER,
                AvatarUrl = ACCOUNT_IMAGE_URL,
                Embeds = [
                        new EmbedBuilder
                        {
                                Title = status.Hostname,
                                Description = $"Screentime for **{today.ToString("d", _culture)}** is used up!",
                                Color = Color.Red,
                                Thumbnail = new EmbedMediaBuilder { Url = THUMBNAIL_URL },
                                Fields = [
                                    new EmbedFieldBuilder {
                                        Name = "Allowed Time",
                                        Value = $"{status.ScreentimeLimit.ToString(TIMESPAN_FORMAT)}",
                                        Inline = true
                                    }
                                ],
                                Footer = new EmbedFooterBuilder { Text = DISPLAYED_USER},
                                Timestamp = DateTime.Now
                        }
                ]
            };
        }

        protected virtual MessageBuilder GetHostRestartedWithoutTimeMsg(HostStatus status)
        {
            var today = DateTime.Today;
            return new MessageBuilder
            {
                Content = $"@everyone",
                Username = DISPLAYED_USER,
                AvatarUrl = ACCOUNT_IMAGE_URL,
                Embeds = [
                        new EmbedBuilder
                        {
                                Title = status.Hostname,
                                Description = $"System was started again **without remaining screentime**!",
                                Color = Color.Red,
                                Thumbnail = new EmbedMediaBuilder { Url = THUMBNAIL_URL },
                                Fields = [
                                    new EmbedFieldBuilder {
                                        Name = "Allowed Time",
                                        Value = $"{status.ScreentimeLimit.ToString(TIMESPAN_FORMAT)}",
                                        Inline = true
                                    },
                                    new EmbedFieldBuilder {
                                        Name = "Exceeded",
                                        Value = $"{status.ScreentimeLeft.Duration().ToString(TIMESPAN_FORMAT)}",
                                        Inline = true
                                    }
                                ],
                                Footer = new EmbedFooterBuilder { Text = DISPLAYED_USER},
                                Timestamp = DateTime.Now
                        }
                ]
            };
        }
    }
}
