using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LunaBot.ServerUtilities
{
    internal static class BotReporting
    {
        private static SocketTextChannel reportChannel;

        public static void SetBotReportingChannel(SocketTextChannel rc)
        {
            reportChannel = rc;
        }

        public static async Task ReportAsync(Color color, SocketTextChannel channel, string title, string content, SocketUser originUser, SocketUser targetUser = null)
        {
            EmbedBuilder eb = new EmbedBuilder();

            EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder();
            authorBuilder.WithName(title);
            //authorBuilder.WithUrl("Title URL");
            authorBuilder.WithIconUrl(originUser.GetAvatarUrl());

            eb.WithAuthor(authorBuilder);
            eb.WithColor(color);
            eb.WithDescription(content);

            eb.WithCurrentTimestamp();

            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            //footer.WithIconUrl("URL to footer image");
            footer.WithText("footer").Build();
            eb.WithFooter(footer);

            //eb.WithTitle("Title");
            eb.WithThumbnailUrl(targetUser.GetAvatarUrl());
            eb.WithUrl("http://EBUrlshow.com");
            
            await reportChannel.SendMessageAsync("",false, eb);
        }
    }

    enum level
    {
        blue,
        yellow,
        red
    }
}
