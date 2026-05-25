using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace FrostHaven
{
    public class GreenhouseManager
    {
        private const int ENERGY_COST = 5;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly EnergyManager _energy;
        private Dictionary<string, List<int>> _pausedCrops = new();

        public GreenhouseManager(IModHelper helper, IMonitor monitor, EnergyManager energy)
        {
            _helper = helper;
            _monitor = monitor;
            _energy = energy;

            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (_energy.CurrentEnergy >= ENERGY_COST)
            {
                _energy.ConsumeEnergy(ENERGY_COST);
                ResumeCrops();
                _monitor.Log($"Greenhouse running: -{ENERGY_COST} energy, crops growing", LogLevel.Info);
            }
            else
            {
                PauseCrops();
                string message = _helper.Translation.Get("energy.low");
                Game1.addHUDMessage(new HUDMessage(message));
                _monitor.Log("Greenhouse stopped: insufficient energy", LogLevel.Warn);
            }
        }

        private void PauseCrops()
        {
            foreach (var location in Game1.locations)
            {
                foreach (var feature in location.terrainFeatures.Values)
                {
                    if (feature is HoeDirt dirt && dirt.crop != null)
                    {
                        string key = $"{location.Name}_{dirt.GetHashCode()}";
                        if (!_pausedCrops.ContainsKey(key))
                        {
                            var crop = dirt.crop;
                            var originalDays = new List<int>(crop.phaseDays);
                            _pausedCrops[key] = originalDays;
                            for (int i = 0; i < crop.phaseDays.Count; i++)
                            {
                                crop.phaseDays[i] = 9999;
                            }
                        }
                    }
                }
            }
        }

        private void ResumeCrops()
        {
            foreach (var location in Game1.locations)
            {
                foreach (var feature in location.terrainFeatures.Values)
                {
                    if (feature is HoeDirt dirt && dirt.crop != null)
                    {
                        string key = $"{location.Name}_{dirt.GetHashCode()}";
                        if (_pausedCrops.TryGetValue(key, out var originalDays))
                        {
                            dirt.crop.phaseDays.Clear();
                            dirt.crop.phaseDays.AddRange(originalDays);
                            _pausedCrops.Remove(key);
                        }
                    }
                }
            }
        }
    }
}
