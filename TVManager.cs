using System;
using System.Collections.Generic;
using FrostHaven.Channels;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace FrostHaven
{
    public class TVManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly NewsChannel _newsChannel;
        private bool _hasShownBroadcast;

        public TVManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _newsChannel = new NewsChannel(helper, monitor);

            var harmony = new Harmony(helper.ModContent.ModID);
            ApplyHarmonyPatches(harmony);

            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void ApplyHarmonyPatches(Harmony harmony)
        {
            try
            {
                _monitor.Log("Applying Harmony patches for TV channel...", LogLevel.Debug);

                // 补丁 1：拦截 TV.checkForAction，替换原版频道列表构建
                var checkForActionMethod = AccessTools.Method(
                    typeof(TV),
                    nameof(TV.checkForAction),
                    new Type[] { typeof(Farmer), typeof(bool) }
                );

                if (checkForActionMethod == null)
                {
                    _monitor.Log("Could not find TV.checkForAction method!", LogLevel.Error);
                    return;
                }

                _monitor.Log($"Found checkForAction method: {checkForActionMethod}", LogLevel.Debug);

                harmony.Patch(
                    original: checkForActionMethod,
                    prefix: new HarmonyMethod(
                        typeof(TVManager),
                        nameof(CheckForAction_Prefix)
                    )
                );

                // 补丁 2：拦截频道选择事件
                var selectChannelMethod = AccessTools.Method(
                    typeof(TV),
                    nameof(TV.selectChannel)
                );

                if (selectChannelMethod == null)
                {
                    _monitor.Log("Could not find TV.selectChannel method!", LogLevel.Error);
                    return;
                }

                _monitor.Log($"Found selectChannel method: {selectChannelMethod}", LogLevel.Debug);

                harmony.Patch(
                    original: selectChannelMethod,
                    prefix: new HarmonyMethod(
                        typeof(TVManager),
                        nameof(SelectChannel_Prefix)
                    )
                );

                _monitor.Log("Harmony patches applied successfully for TV channel", LogLevel.Info);
                _monitor.Log($"Channel ID: {NewsChannel.ChannelID}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error applying Harmony patches: {ex.Message}", LogLevel.Error);
                _monitor.Log($"Stack trace: {ex.StackTrace}", LogLevel.Debug);
            }
        }

        // 补丁 1：替换 TV.checkForAction，构建包含自定义频道的频道列表
        private static bool CheckForAction_Prefix(TV __instance, Farmer who, bool justCheckingForActivity = false)
        {
            try
            {
                if (justCheckingForActivity)
                    return true;

                Instance._monitor.Log("=== CheckForAction_Prefix called! ===", LogLevel.Info);

                // 构建频道列表（复刻原版逻辑）
                var list = new List<Response>();
                list.Add(new Response("Weather", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13105")));
                list.Add(new Response("Fortune", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13107")));

                switch (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
                {
                    case "Mon":
                    case "Thu":
                        list.Add(new Response("Livin'", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13111")));
                        break;
                    case "Sun":
                        list.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13114")));
                        break;
                    case "Wed":
                        if (Game1.stats.DaysPlayed > 7)
                            list.Add(new Response("The", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13117")));
                        break;
                }

                if (Game1.Date.Season == Season.Fall && Game1.Date.DayOfMonth == 26
                    && Game1.stats.Get("childrenTurnedToDoves") != 0
                    && !who.mailReceived.Contains("cursed_doll"))
                {
                    list.Add(new Response("???", "???"));
                }

                if (Game1.player.mailReceived.Contains("pamNewChannel"))
                {
                    list.Add(new Response("Fishing", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV_Fishing_Channel")));
                }

                // 插入自定义频道（在 Leave 之前）
                string channelTitle = Instance._helper.Translation.Get("tv.channel_title");
                list.Add(new Response(NewsChannel.ChannelID, channelTitle));
                Instance._monitor.Log($"Added channel: {channelTitle} (key={NewsChannel.ChannelID})", LogLevel.Info);

                list.Add(new Response("(Leave)", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13118")));

                // 手动调用 createQuestionDialogue
                Game1.currentLocation.createQuestionDialogue(
                    Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120"),
                    list.ToArray(),
                    __instance.selectChannel
                );
                Game1.player.Halt();

                Instance._monitor.Log($"Channel list built with {list.Count} options", LogLevel.Debug);

                return false; // 跳过原版 checkForAction
            }
            catch (Exception ex)
            {
                Instance._monitor.Log($"Error in CheckForAction_Prefix: {ex.Message}", LogLevel.Error);
                Instance._monitor.Log($"Stack trace: {ex.StackTrace}", LogLevel.Debug);
                return true; // 出错时回退到原版处理
            }
        }

        // 补丁 2：处理自定义频道选择
        private static bool SelectChannel_Prefix(TV __instance, Farmer who, string answer)
        {
            try
            {
                Instance._monitor.Log($"selectChannel called with answer: {answer}", LogLevel.Debug);

                if (answer == NewsChannel.ChannelID)
                {
                    Instance._monitor.Log("FrostHaven news channel selected!", LogLevel.Info);
                    Instance._newsChannel.Show(__instance);
                    return false; // 阻止原版处理
                }

                return true; // 继续原版处理
            }
            catch (Exception ex)
            {
                Instance._monitor.Log($"Error in SelectChannel_Prefix: {ex.Message}", LogLevel.Error);
                return true;
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.dayOfMonth == 1 && Game1.currentSeason == "spring" && !_hasShownBroadcast)
            {
                _hasShownBroadcast = true;
                _monitor.Log("Day 1 Spring: News channel now available", LogLevel.Info);
                Game1.addHUDMessage(new HUDMessage(_helper.Translation.Get("tv.emergency_news")));
            }
        }

        private static TVManager _instance;
        private static TVManager Instance => _instance ??= new TVManager(
            ModEntry.Instance.Helper,
            ModEntry.Instance.Monitor
        );
    }
}
