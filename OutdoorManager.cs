using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FrostHaven
{
    /// <summary>
    /// 户外寒冷伤害系统。
    /// 室外温度随天数渐变，按温度区间每 10 分钟扣血。
    /// 有寒冷免疫 buff 时跳过扣血。
    /// </summary>
    public class OutdoorManager
    {
        // 温度曲线常量
        private const double START_TEMP = 10.0;      // Day 1 起始温度
        private const double DAY10_TEMP = -20.0;     // Day 10 温度
        private const double DAY30_TEMP = -50.0;     // Day 30 温度
        private const int WARMUP_DAYS = 10;          // 渐冷第一阶段天数
        private const int FREEZE_DAYS = 30;          // 极寒达到天数

        // 温度区间与扣血速率（每 10 分钟）
        private const double SAFE_TEMP = 10.0;
        private const double LIGHT_TEMP = -10.0;
        private const double MEDIUM_TEMP = -30.0;

        private const int HP_SAFE = 0;
        private const int HP_LIGHT = 1;
        private const int HP_MEDIUM = 5;
        private const int HP_HEAVY = 15;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public OutdoorManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null)
                return;

            bool isOutdoors = Game1.player.currentLocation?.IsOutdoors ?? false;
            if (!isOutdoors)
                return;

            // 有寒冷免疫 buff 时跳过
            if (Game1.player.hasBuff(ColdImmunityBuffManager.BuffId))
                return;

            double temp = GetOutdoorTemperature();
            int hp = GetHpDeduction(temp);

            if (hp > HP_SAFE)
            {
                Game1.player.health -= hp;
                string message = string.Format(_helper.Translation.Get("outdoor.freezing"), hp);
                Game1.addHUDMessage(new HUDMessage(message));
                _monitor.Log($"Player freezing at {temp:F1}°C: -{hp} HP, current HP: {Game1.player.health}", LogLevel.Warn);
            }
        }

        /// <summary>
        /// 根据当前天数计算室外温度。
        /// Day 1-10: 10°C → -20°C（线性）
        /// Day 11-30: -20°C → -50°C（线性）
        /// Day 30+: 固定 -50°C
        /// </summary>
        public static double GetOutdoorTemperature()
        {
            int day = Game1.dayOfMonth;
            string season = Game1.currentSeason;

            // 计算总天数（年份内累计天数）
            int totalDay = GetTotalDayInYear(season, day);

            if (totalDay <= WARMUP_DAYS)
            {
                // Day 1-10: 10°C → -20°C
                return START_TEMP + (DAY10_TEMP - START_TEMP) * (totalDay - 1) / (WARMUP_DAYS - 1);
            }
            else if (totalDay <= FREEZE_DAYS)
            {
                // Day 11-30: -20°C → -50°C
                return DAY10_TEMP + (DAY30_TEMP - DAY10_TEMP) * (totalDay - WARMUP_DAYS) / (FREEZE_DAYS - WARMUP_DAYS);
            }
            else
            {
                return DAY30_TEMP;
            }
        }

        /// <summary>
        /// 获取当前年份内的累计天数。
        /// </summary>
        private static int GetTotalDayInYear(string season, int day)
        {
            int seasonOffset = season switch
            {
                "spring" => 0,
                "summer" => 28,
                "fall" => 56,
                "winter" => 84,
                _ => 0
            };
            return seasonOffset + day;
        }

        /// <summary>
        /// 根据温度返回每 10 分钟应扣的 HP。
        /// </summary>
        public static int GetHpDeduction(double temp)
        {
            if (temp >= SAFE_TEMP) return HP_SAFE;
            if (temp > LIGHT_TEMP) return HP_LIGHT;
            if (temp > MEDIUM_TEMP) return HP_MEDIUM;
            return HP_HEAVY;
        }
    }
}
