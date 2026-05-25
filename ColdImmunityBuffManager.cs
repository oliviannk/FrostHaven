using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;

namespace FrostHaven
{
    /// <summary>
    /// 检测玩家饮用 Coffee / Green Tea，自动添加寒冷免疫 buff。
    /// </summary>
    public class ColdImmunityBuffManager
    {
        public const string BuffId = "FrostHaven.ColdImmunity";

        private const string COFFEE_ID = "(O)395";
        private const string GREEN_TEA_ID = "(O)614";

        // 游戏内时间：1 小时 = 60 分钟，1 分钟 = 700ms
        private const int COFFEE_DURATION_MS = 90 * 700;    // 1.5 游戏小时
        private const int GREEN_TEA_DURATION_MS = 180 * 700; // 3 游戏小时

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private Texture2D _buffIcon;

        private bool _wasEating;

        public ColdImmunityBuffManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            _buffIcon = helper.ModContent.Load<Texture2D>("assets/buff-icon.png");

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            bool isEating = Game1.player.isEating;

            // 上升沿检测：刚从不吃变为吃
            if (isEating && !_wasEating)
            {
                Item food = Game1.player.itemToEat;
                if (food != null)
                {
                    TryApplyColdImmunity(food);
                }
            }

            _wasEating = isEating;
        }

        private void TryApplyColdImmunity(Item item)
        {
            string itemId = item.QualifiedItemId;
            int duration;
            string buffMessage;

            if (itemId == COFFEE_ID)
            {
                duration = COFFEE_DURATION_MS;
                buffMessage = _helper.Translation.Get("buff.coldimmunity.coffee");
            }
            else if (itemId == GREEN_TEA_ID)
            {
                duration = GREEN_TEA_DURATION_MS;
                buffMessage = _helper.Translation.Get("buff.coldimmunity.greentea");
            }
            else
            {
                return;
            }

            string buffName = _helper.Translation.Get("buff.coldimmunity.name");

            var buff = new Buff(
                id: BuffId,
                source: "FrostHaven",
                displayName: buffName,
                duration: duration,
                iconTexture: _buffIcon,
                iconSheetIndex: 0,
                effects: new BuffEffects()
            );

            Game1.player.applyBuff(buff);
            Game1.addHUDMessage(new HUDMessage(buffMessage));
            _monitor.Log($"Cold immunity applied: {itemId}, duration {duration}ms", LogLevel.Info);
        }
    }
}
