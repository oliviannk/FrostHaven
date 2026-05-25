using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FrostHaven
{
    /// <summary>
    /// 开局赠送物资：Day 1 Spring 显示爷爷的信件，关闭后物品自动进入背包。
    /// </summary>
    public class StartingGiftManager
    {
        private const string MAIL_ID = "FrostHaven.StartingGift";

        // 原版物品 ID
        private const string COFFEE_ID = "(O)395";
        private const int COFFEE_COUNT = 5;

        private const string GREEN_TEA_ID = "(O)614";
        private const int GREEN_TEA_COUNT = 3;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        private bool _pendingGift;

        public StartingGiftManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;

                    // 信件内容不含物品附件，仅显示文字
                    string body = _helper.Translation.Get("starting_gift.body");
                    string title = _helper.Translation.Get("starting_gift.title");
                    string letter = $"{body}[#]{title}";

                    data[MAIL_ID] = letter;
                    _monitor.Log($"Registered starting gift mail: {MAIL_ID}", LogLevel.Trace);
                });
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            // 仅 Day 1 Spring 触发
            if (Game1.dayOfMonth != 1 || Game1.currentSeason != "spring")
                return;

            // 避免重复
            if (Game1.player.mailReceived.Contains(MAIL_ID))
                return;

            // 打开信件查看器
            string body = _helper.Translation.Get("starting_gift.body");
            string title = _helper.Translation.Get("starting_gift.title");
            string letter = $"{body}[#]{title}";

            Game1.activeClickableMenu = new LetterViewerMenu(letter);

            // 标记待发礼物（信件关闭后添加物品）
            _pendingGift = true;

            // 标记已读，防止再次触发
            Game1.player.mailReceived.Add(MAIL_ID);

            _monitor.Log("Starting gift letter opened", LogLevel.Info);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!_pendingGift)
                return;

            // 信件菜单已关闭，添加物品到背包
            if (Game1.activeClickableMenu is not LetterViewerMenu)
            {
                _pendingGift = false;

                var coffee = ItemRegistry.Create(COFFEE_ID, COFFEE_COUNT);
                Game1.player.addItemByMenuIfNecessary(coffee);

                var greenTea = ItemRegistry.Create(GREEN_TEA_ID, GREEN_TEA_COUNT);
                Game1.player.addItemByMenuIfNecessary(greenTea);

                _monitor.Log($"Starting gift added: Coffee x{COFFEE_COUNT}, Green Tea x{GREEN_TEA_COUNT}", LogLevel.Info);
            }
        }
    }
}
