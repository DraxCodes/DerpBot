using System;
using DSharpPlus;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace DerpBot
{
    class Program
    {
        static DiscordClient Client { get; set; }
        public InteractivityModule Interactivity { get; set; }
        public CommandsNextModule Commands { get; set; }
        static VoiceNextClient Voice;
        public static DateTime startTime = DateTime.Now;

        static void Main(string[] args)
        {
            var prog = new Program();
            prog.MainAsync().GetAwaiter().GetResult();
            //MainAsync(args).GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            #region Config Stuffs
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();
            var cfgJson = JsonConvert.DeserializeObject<Config>(json);
            var cfg = new DiscordConfiguration
            {
                Token = cfgJson.BotToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
            #endregion

            Client = new DiscordClient(cfg);
            Voice = Client.UseVoiceNext();

            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;


            Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = TimeoutBehaviour.Ignore,
                PaginationTimeout = TimeSpan.FromMinutes(5),    
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandcfg = new CommandsNextConfiguration
            {
                StringPrefix = cfgJson.Prefix,
                EnableDms = false,
                EnableMentionPrefix = true
            };

            Commands = Client.UseCommandsNext(commandcfg);
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            Commands.RegisterCommands<UtilityCommands>();
            Commands.RegisterCommands<Guides>();
            Commands.RegisterCommands<Music>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        

        private Task Client_Ready(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DraxBot", "Client is ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DraxBot", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "DraxBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "DraxBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "DraxBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning~1:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access Denied",
                    Description = $"{emoji} You do not have the required permisions for that command.",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync("", embed: embed);
            }

            if (e.Exception is CommandNotFoundException)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Unknow Command",
                    Description = $"I don't seem to know what that command is.\nPlease use ``//help`` to view my commands.",
                    Color = new DiscordColor(0xFF0000)
                };
                await e.Context.RespondAsync("", embed: embed);
            }

        }
    }
}
