using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using Octokit;

namespace DerpBot
{
    public class UtilityCommands
    {
        [Command("sudo")][RequireOwner][Hidden]
        [Description("Use a command as a defined user.")]
        public async Task SudoCommand(CommandContext ctx, [Description("Member to execute as.")] DiscordMember user, [Description("Command to Execute")][RemainingText] string command = null)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                var cmds = ctx.CommandsNext;
                if (command == null) await ctx.RespondAsync("Please enter a command, command can not be null. **See ``!help sudo``**");
                else { await cmds.SudoAsync(user, ctx.Channel, command); await ctx.RespondAsync($"Successfully used {command} as {user.Username}"); }
            }
            catch (Exception ex)
            {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "DraxBot", $"Exception occured: {ex.Message}", DateTime.Now);
                throw;
            }
        }

        [Command("nick")][RequirePermissions(Permissions.ManageNicknames)]
        [Description("Changes a users nickname (Requires Role: ManageNicknames)")]
        public async Task Nickname(CommandContext ctx, [Description("The user you wish to change the nickname of.")] DiscordMember user, [Description("The nickname you wish to set for the user.")][RemainingText]string nickname = null)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                if (nickname == null) await ctx.RespondAsync($"There was an error, Please provide a nickname to set for {user.Username} **(More Info via ``!help nick``)**");
                else { await user.ModifyAsync(nickname, reason: $"Changed by {ctx.User.Username} ({ctx.User.Id})"); await ctx.RespondAsync($"Successfully changed {user.Username}'s name to {nickname}."); }

            }
            catch (Exception ex)
            {
                
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "DraxBot", $"Exception occured: {ex.Message}", DateTime.Now);
                await ctx.RespondAsync("There was an error, Please contact your server Admin or Draxis");
                throw;
            }
        }

        [Command("server")]
        [Description("Gets info about the discord server the command is used in.")]
        public async Task ServerInfo(CommandContext ctx)
        {
            var builder = new DiscordEmbedBuilder()
                .WithTitle($"Server Info | #{ctx.Guild.Name} (Id: {ctx.Guild.Id})")
                .WithColor(new DiscordColor(138, 43, 226))
                .AddField($"Channels", $"• {ctx.Guild.Channels.Where(x => x.Type == ChannelType.Text).Count()} Text, {ctx.Guild.Channels.Where(x => x.Type == ChannelType.Voice).Count()} Voice" +
                $"\n• AFK: {ctx.Guild.AfkTimeout / 60} Min", true)
                .AddField($"Member", $"• {ctx.Guild.MemberCount} Members" +
                $"\n• Owner: {ctx.Guild.Owner.Mention}", true)
                .AddField("Other", $"• Roles: {ctx.Guild.Roles.Count}" +
                $"\n• Region: {ctx.Guild.RegionId}" +
                $"\n• Created On: {ctx.Guild.CreationTimestamp.ToString("dddd, MMMM d, yyyy @ h:mm tt")}" +
                $"\n• Verifcation Level: {ctx.Guild.VerificationLevel}", false)
                .WithThumbnailUrl(ctx.Guild.IconUrl);

            await ctx.RespondAsync("", false, builder);
        }

        [Command("game")][RequireOwner]
        [Description("Sets the now playing game of the bot. (Requires Owner)")]
        public async Task UpdateStatusAsync(CommandContext ctx, [Description("What you want to display as the now playing for the bot.")]string game)
        {
            UserStatus? user_status = default(UserStatus?);
            DateTimeOffset? idle_since = default(DateTimeOffset?);

            await ctx.Client.UpdateStatusAsync(new DiscordGame(game), user_status , idle_since);
        }

        [Command("info")]
        [Description("Gets info about the bot.")]
        public async Task BotInfo(CommandContext ctx)
        {
            var owner = "joelp53";
            var reponame = "DerpBot";
            var client = new GitHubClient(new ProductHeaderValue("DerpBot"));
            var repo = await client.Repository.Get(owner, reponame);
            var botUptime = DateTime.Now - Program.startTime; StringBuilder replyUptime = new StringBuilder();

            #region UpitimeStuffs
            if (botUptime.Days > 0 && botUptime.Days < 2) replyUptime.Append($"{botUptime.Days} Day, ");
            if (botUptime.Days > 1) replyUptime.Append($"{botUptime.Days} Days, ");
            if (botUptime.Hours > 0 && botUptime.Hours < 2) replyUptime.Append($"{botUptime.Hours} Hour, ");
            if (botUptime.Hours > 1) replyUptime.Append($"{botUptime.Hours} Hours, ");
            if (botUptime.Minutes > 0 && botUptime.Minutes < 2) replyUptime.Append($"{botUptime.Minutes} Min, ");
            if (botUptime.Minutes > 1) replyUptime.Append($"{botUptime.Minutes} Mins, ");
            if (botUptime.Seconds > 0) replyUptime.Append($"{botUptime.Seconds} Secconds");
            #endregion

            var builder = new DiscordEmbedBuilder()
                .WithTimestamp(DateTime.UtcNow)
                .WithColor(new DiscordColor(138, 43, 226))
                .WithTitle("DerpBot Information")
                .WithUrl($"{repo.HtmlUrl}")
                .WithDescription($"DerpBot was created by Draxis with the main idea being to replace the previous bot DraxBot.")
                .AddField("Latest Features: ", 
                "**• World Of Warcraft Class Help:**\n" +
                "Find how to use this command via ``//help guide``\n" +
                "**• Youtube Music:**\n" +
                "Have DerpBot join your voice channel ``//join``\n" +
                "Play music with ``//play [youtube link]``")
                .AddField("Upcoming Features:", "``Wow Stats`` | ``Raider.IO Info`` | ``Overwatch Stats`` | ``Better Music``\nCurrently Accepting Requests for more commands.")
                .AddField("Stats:", 
                $"**• Uptime:** {replyUptime.ToString()}\n" +
                $"**• Created on:** {repo.CreatedAt.ToString("dddd, MMMM d, yyyy @ h:mm tt")}")
                .WithThumbnailUrl(ctx.Client.CurrentUser.AvatarUrl)
                .WithFooter($"This Embed works best when viewed on a PC.", ctx.Client.CurrentUser.AvatarUrl);

            await ctx.RespondAsync("", false, builder);
        }
    }
}