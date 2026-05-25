using System;

namespace FrostHaven.Utils
{
    /// <summary>
    /// 能量计算工具类（可独立测试）
    /// </summary>
    public static class EnergyCalculator
    {
        /// <summary>
        /// 计算每日能量产出
        /// </summary>
        /// <param name="currentEnergy">当前能量</param>
        /// <param name="production">每日产出量</param>
        /// <param name="maxEnergy">能量上限</param>
        /// <returns>新的能量值</returns>
        public static int CalculateDailyProduction(int currentEnergy, int production, int maxEnergy)
        {
            return Math.Min(currentEnergy + production, maxEnergy);
        }

        /// <summary>
        /// 消耗能量
        /// </summary>
        /// <param name="currentEnergy">当前能量</param>
        /// <param name="cost">消耗量</param>
        /// <returns>消耗后的能量值（如果能量不足，返回当前值）</returns>
        public static int ConsumeEnergy(int currentEnergy, int cost)
        {
            if (currentEnergy < cost)
                return currentEnergy;
            return currentEnergy - cost;
        }

        /// <summary>
        /// 检查能量是否足够
        /// </summary>
        /// <param name="currentEnergy">当前能量</param>
        /// <param name="cost">需要的能量</param>
        /// <returns>是否足够</returns>
        public static bool HasEnoughEnergy(int currentEnergy, int cost)
        {
            return currentEnergy >= cost;
        }

        /// <summary>
        /// 检查是否应该暂停作物（能量不足）
        /// </summary>
        /// <param name="currentEnergy">当前能量</param>
        /// <param name="minEnergy">最低能量阈值</param>
        /// <returns>是否应该暂停</returns>
        public static bool ShouldPauseCrops(int currentEnergy, int minEnergy = 0)
        {
            return currentEnergy <= minEnergy;
        }
    }
}
