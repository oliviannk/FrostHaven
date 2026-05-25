using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FrostHaven
{
    public class EnergyModel
    {
        public int Value { get; set; } = 20;
    }

    public class EnergyManager
    {
        private const int MAX_ENERGY = 50;
        private const int DAILY_PRODUCTION = 10;

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private EnergyModel _model;

        public int CurrentEnergy => _model.Value;

        public EnergyManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.Saving += OnSaving;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _model = _helper.Data.ReadSaveData<EnergyModel>("frosthaven_energy")
                     ?? new EnergyModel();
            _monitor.Log($"Energy loaded: {_model.Value}/{MAX_ENERGY}", LogLevel.Info);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            _model.Value = Math.Min(_model.Value + DAILY_PRODUCTION, MAX_ENERGY);
            _helper.Data.WriteSaveData("frosthaven_energy", _model);

            string message = _helper.Translation.Get("energy.daily_report", new { value = _model.Value });
            Game1.addHUDMessage(new HUDMessage(message));

            _monitor.Log($"Daily energy: +{DAILY_PRODUCTION}, current {_model.Value}/{MAX_ENERGY}", LogLevel.Info);
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            _helper.Data.WriteSaveData("frosthaven_energy", _model);
        }

        public bool ConsumeEnergy(int amount)
        {
            if (_model.Value < amount)
            {
                _monitor.Log($"Energy insufficient: need {amount}, have {_model.Value}", LogLevel.Warn);
                return false;
            }
            _model.Value -= amount;
            _helper.Data.WriteSaveData("frosthaven_energy", _model);
            _monitor.Log($"Energy consumed: -{amount}, current {_model.Value}/{MAX_ENERGY}", LogLevel.Info);
            return true;
        }

        public void AddEnergy(int amount)
        {
            _model.Value = Math.Min(_model.Value + amount, MAX_ENERGY);
            _helper.Data.WriteSaveData("frosthaven_energy", _model);
            _monitor.Log($"Energy added: +{amount}, current {_model.Value}/{MAX_ENERGY}", LogLevel.Info);
        }
    }
}
