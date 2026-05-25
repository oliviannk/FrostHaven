using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FrostHaven
{
    /// <summary>
    /// 强制所有季节视觉替换为冬季。
    /// </summary>
    public class WinterVisualManager
    {
        private static readonly string[] SeasonalTileSheets = new[]
        {
            "outdoorsTileSheet",
            "outdoorsTileSheet2",
            "town",
            "beach",
            "Shadows",
            "Waterfalls"
        };

        // 地形特征贴图（路径格式不同：TileSheets/xxx）
        private static readonly string[] TerrainFeatureSheets = new[]
        {
            "TileSheets/trees",
            "TileSheets/grass",
            "TileSheets/Bushes"
        };

        private static readonly string[] NonWinterSeasons = new[] { "spring", "summer", "fall" };

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public WinterVisualManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            _monitor.Log("WinterVisualManager initialized: always-winter mode active", LogLevel.Info);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // 失效当前季节的贴图缓存，强制 AssetRequested 重新触发
            string season = Game1.currentSeason;
            if (season != "winter")
            {
                foreach (string sheet in SeasonalTileSheets)
                {
                    _helper.GameContent.InvalidateCache($"Maps/{season}_{sheet}");
                }
                foreach (string sheet in TerrainFeatureSheets)
                {
                    _helper.GameContent.InvalidateCache(sheet);
                }
                _monitor.Log($"Invalidated {season} tile sheet caches", LogLevel.Trace);
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            // 当前是冬季，不需要替换
            if (Game1.currentSeason == "winter")
                return;

            // 1. 地图 tilesheet 替换（Maps/{season}_{sheet} → Maps/winter_{sheet}）
            foreach (string sheet in SeasonalTileSheets)
            {
                foreach (string season in NonWinterSeasons)
                {
                    string sourceAsset = $"Maps/{season}_{sheet}";
                    if (!e.NameWithoutLocale.IsEquivalentTo(sourceAsset))
                        continue;

                    string winterAsset = $"Maps/winter_{sheet}";
                    e.LoadFrom(
                        () => _helper.GameContent.Load<Texture2D>(winterAsset),
                        AssetLoadPriority.Exclusive
                    );
                    _monitor.Log($"Redirected {sourceAsset} → {winterAsset}", LogLevel.Trace);
                    return;
                }
            }

            // 2. 地形特征贴图替换（TileSheets/xxx → 冬季版本）
            //    这些贴图可能以单文件形式存在（内含多季节行），
            //    也可能有季节后缀（如 trees_spring）。两种都尝试拦截。
            foreach (string sheet in TerrainFeatureSheets)
            {
                // 尝试带季节后缀的格式：TileSheets/trees_spring → TileSheets/trees_winter
                foreach (string season in NonWinterSeasons)
                {
                    string sourceWithSeason = $"{sheet}_{season}";
                    if (e.NameWithoutLocale.IsEquivalentTo(sourceWithSeason))
                    {
                        string winterVersion = $"{sheet}_winter";
                        e.LoadFrom(
                            () => _helper.GameContent.Load<Texture2D>(winterVersion),
                            AssetLoadPriority.Exclusive
                        );
                        _monitor.Log($"Redirected {sourceWithSeason} → {winterVersion}", LogLevel.Trace);
                        return;
                    }
                }
            }
        }
    }
}
