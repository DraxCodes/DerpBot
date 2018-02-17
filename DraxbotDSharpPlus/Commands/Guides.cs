using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;

namespace DerpBot
{
    public class Guides
    {
        [Command("guide")]
        [Description("Pulls basic guide information for a desired class.")]
        public async Task WowStats(CommandContext ctx, [Description("The spec of the class you want to look up.")]string specInput, [Description("The Class you want to look up.")] string classInput)
        {
            if (string.IsNullOrEmpty(specInput)) await ctx.RespondAsync("Please enter a spec, spec can not be null. **See ``!help guide``**");
            if (string.IsNullOrEmpty(classInput)) await ctx.RespondAsync("Please enter a class, class can not be null. **See ``!help guide``**");

            string checked_spec = WOWGuide_Spec.Check(specInput.ToLower()); string checked_class = WOWGuide_Class.Check(classInput.ToLower());

            await ctx.TriggerTypingAsync();
            var replyBuilder = new DiscordEmbedBuilder();


            try
            {
                WOWGuide.ParseData(checked_spec, checked_class);
                var results = WOWGuide_Result.Results;
                var tierToken = WOWGuide_Result.Token;

                

                if (results == null || tierToken == null)
                {
                    var Failbuilder = new DiscordEmbedBuilder()
                                   .WithTitle("ERROR")
                                   .WithFooter("Powered by Draxbot")
                                   .WithDescription("There seems to be an issue with your command ``!guide``. Please ensure you are using it correcty: ``!guide [spec] [class]``")
                                   .WithTimestamp(DateTime.UtcNow);
                    Failbuilder.Build(); await ctx.RespondAsync("", false, Failbuilder);
                }

                replyBuilder.WithTimestamp(DateTime.UtcNow)
                    .WithColor(new DiscordColor(64, 224, 208))
                    .WithTitle(results.Title)
                    .WithDescription(results.Description)
                    .WithUrl(results.Link)
                    .WithFooter("Powered By DraxBot & Icy Veins | See a problem? PM Draxis")
                    .AddField("Stat Priority", $"{results.Stats}")
                    .AddField("Shares Tier Tokens With", tierToken)
                    .AddField("Note", "Remember these are only basic stat priorities, you should ALWAYS Sim yourself if possible too!")
                    .AddField("Links", $"[Link To Icy Veins Guide]({results.Link}) | [Link To RaidBot, Sim yourself!](https://www.raidbots.com/)")
                    .WithThumbnailUrl("https://cdn2.iconfinder.com/data/icons/basic-office-snippets/170/Basic_Office-9-512.png");

                replyBuilder.Build(); await ctx.RespondAsync("", false, replyBuilder);

                WOWGuide_Result.Results = null; WOWGuide_Result.Token = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                throw;
            }
        }
    }
}
