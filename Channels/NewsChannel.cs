using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace FrostHaven.Channels
{
    public class NewsChannel
    {
        // 使用唯一的频道 ID，格式：作者.Mod名.频道名
        public static readonly string ChannelID = "Ann.FrostHaven.News";
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public NewsChannel(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public void Show(TV tv)
        {
            try
            {
                _monitor.Log("NewsChannel.Show() called", LogLevel.Debug);

                // 获取翻译后的新闻内容
                string newsContent = _helper.Translation.Get("tv.broadcast");
                string title = _helper.Translation.Get("tv.channel_title");

                _monitor.Log($"Channel title: {title}", LogLevel.Debug);
                _monitor.Log($"News content length: {newsContent.Length} characters", LogLevel.Debug);

                // 显示新闻内容
                Game1.drawObjectDialogue($"{title}\n\n{newsContent}");

                _monitor.Log("FrostHaven news channel displayed successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error showing news channel: {ex.Message}", LogLevel.Error);
                _monitor.Log($"Stack trace: {ex.StackTrace}", LogLevel.Debug);
            }
        }
    }
}
