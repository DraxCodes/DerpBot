using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using System.Threading;
using YoutubeExplode.Models;

namespace DerpBot
{
    public class Music
    {
        private static readonly string TempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "TempAudio");
        private static CancellationTokenSource ctsStop;
        private static bool skip;
        static List<Video> playList = new List<Video>();

        [Command("join")]
        [Description("Use this command to get the bot to join your voice channel.")]
        public async Task Join(CommandContext ctx)
        {
            var vNextClient = ctx.Client.GetVoiceNextClient();
            var vNextConnection = vNextClient.GetConnection(ctx.Guild);
            var vNextChannel = ctx.Member?.VoiceState?.Channel;
            if (vNextConnection != null) await ctx.RespondAsync($"Already Connected in this guild. ({ctx.Guild.Name})");
            if (vNextChannel == null) await ctx.RespondAsync($"You need to be in a voice channel first.");

            vNextConnection = await vNextClient.ConnectAsync(vNextChannel);
            await ctx.RespondAsync($"I am now connected to ({vNextChannel.Name}).");
        }

        [Command("leave")]
        [Aliases("getlost")]
        [Description("Use this command to make the bot leave the channel.")]
        public async Task Leave(CommandContext ctx)
        {
            var vNextClient = ctx.Client.GetVoiceNextClient();
            var vNextConnection = vNextClient.GetConnection(ctx.Guild);
            if (vNextConnection == null) await ctx.RespondAsync("I am not in any channel in the guild.");

            vNextConnection.Disconnect();
            await ctx.RespondAsync($"I have now left the audio channel.");
        }

        [Command("play")]
        [Aliases("music", "song")]
        [Description("This command can play youtube songs upon request.\nCurrently que's songs as requested. \nCan Que Playlists.\nCan Skip Upon Request.\n**(Currently a Work In Progress)**")]
        public async Task Play(CommandContext ctx, [Description("the url of a youtube music video you wish to play.")][RemainingText] string song)
        {
           // if (song.Contains("&list=")) { await ctx.RespondAsync("I don't currently accept playlist. Sorry :("); return; }
            if (!song.Contains("youtu")) { await ctx.RespondAsync("That doesn't seem to be a valid youtube URL. If you think this is an error, contact Draxis."); return; }
            ctsStop = new CancellationTokenSource();

            var vNextClient = ctx.Client.GetVoiceNextClient();
            var vNextConnection = vNextClient.GetConnection(ctx.Guild);
            if (vNextConnection == null) { await ctx.RespondAsync("I am not in any channel in the guild. Type ``//join`` for me to join a voice channel."); return; }
            var ytClient = new YoutubeClient();
            Video ytInfo = null;
            string ID;
            

            if (song.Contains("&list="))
            {
                ID = YoutubeClient.ParsePlaylistId(song);
                var ytPlaylist = await ytClient.GetPlaylistAsync(ID);
                await ctx.RespondAsync($"Now adding {ytPlaylist.Videos.Count} songs from ``{ytPlaylist.Title}``. Please Wait....");
                foreach (var item in ytPlaylist.Videos)
                {
                    playList.Add(item);
                }
            }
            else
            {
                ID = YoutubeClient.ParseVideoId(song);
                ytInfo = await ytClient.GetVideoAsync(ID);
                playList.Add(ytInfo);
                await ctx.RespondAsync($"Added song: {ytInfo.Title} - Author: {ytInfo.Author}");
            }


            
            while (vNextConnection.IsPlaying)
                await vNextConnection.WaitForPlaybackFinishAsync();

            foreach (var item in playList.ToList())
            {
                skip = false;
                if (ctsStop.IsCancellationRequested) return;
                await vNextConnection.SendSpeakingAsync(true);
                var set = await ytClient.GetVideoMediaStreamInfosAsync(item.Id);
                var streamInfo = GetBestAudioStreamInfo(set);
                var streamFileExt = streamInfo.Container.GetFileExtension();
                var streamFilePath = Path.Combine(TempDirectoryPath, $"{Guid.NewGuid()}.{streamFileExt}");
                await ytClient.DownloadMediaStreamAsync(streamInfo, streamFilePath);
                var ytEmbded = new DiscordEmbedBuilder()
                    .WithTitle($"Now Playing: ({item.Title})")
                    .WithDescription($"Author: {item.Author} | Duration: {item.Duration}")
                    .WithUrl($"{item.GetUrl()}")
                    .WithThumbnailUrl($"{item.Thumbnails.MediumResUrl}");
                await ctx.RespondAsync("", false, ytEmbded);
                playList.Remove(item);

                

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $@"-i ""{streamFilePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                var ffout = ffmpeg.StandardOutput.BaseStream;
                var buff = new byte[3840];
                var br = 0;
                while ((br = ffout.Read(buff, 0, buff.Length)) > 0 && !ctsStop.IsCancellationRequested && skip != true)
                {
                    if (br < buff.Length)
                    {
                        for (var i = br; i < buff.Length; i++)
                        {
                            buff[i] = 0;
                        }

                    }
                    await vNextConnection.SendAsync(buff, 20);
                    
                }
                await vNextConnection.SendSpeakingAsync(false);
                while (vNextConnection.IsPlaying)
                    await vNextConnection.WaitForPlaybackFinishAsync();
                ffout.Flush(); ffout.Close(); ffmpeg.Close();

                
                //Console.WriteLine("Deleting temp file...");
               // File.Delete(streamFilePath);
            }
        }

        [Command("skip")]
        [Description("Skips a song in the que")]
        public async Task Skip(CommandContext ctx)
        {
            skip = true;
            await ctx.RespondAsync("Song has been skipped.");
            if (playList.Count == 0) await ctx.RespondAsync("End of playlist reached.");
        }

        [Command("stop")]
        [Description("Stops the music and clear the queue.")]
        public async Task Stop(CommandContext ctx)
        {
            ctsStop.Cancel();
            playList.Clear();

            await ctx.RespondAsync("Music playback has stopped.");
        }

        [Command("playlist")]
        [Aliases("list")]
        public async Task Playlist(CommandContext ctx)
        {
            if (playList.Count <= 0)
            {
                await ctx.RespondAsync("Playlist is empty!");
                return;
            }

            var sb = new StringBuilder();
            var ytEmbded = new DiscordEmbedBuilder()
                .WithTitle($"Playlist | Next 15 or less Songs");
           
            foreach (var item in playList.Take(15))
            {
                sb.Append($"[{item.Title}]({item.GetUrl()})\n");
            }
            ytEmbded.WithDescription(sb.ToString());
            await ctx.RespondAsync("", false, ytEmbded);
        }

        private static MediaStreamInfo GetBestAudioStreamInfo(MediaStreamInfoSet set)
        {
            if (set.Audio.Any())
                return set.Audio.WithHighestBitrate();
            if (set.Muxed.Any())
                return set.Muxed.WithHighestVideoQuality();
            throw new Exception("No applicable media streams found for this video");
        }

    }
}
