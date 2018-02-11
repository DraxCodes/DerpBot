using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;

namespace DraxbotDSharpPlus
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

        [Command("nick")]
        [Description("Changes a users nickname (Requires Role: ManageNicknames)")]
        [RequirePermissions(Permissions.ManageNicknames)]
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

        [Command("info")]
        [Description("Gets info about the discord server the command is used in.")]
        public async Task Information(CommandContext ctx)
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
    }
}