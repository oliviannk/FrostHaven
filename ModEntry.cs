using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FrostHaven
{
    public class ModEntry : Mod
    {
        // 提供静态实例访问
        internal static ModEntry Instance { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            var winterVisual = new WinterVisualManager(helper, this.Monitor);
            var startingGift = new StartingGiftManager(helper, this.Monitor);
            var coldImmunity = new ColdImmunityBuffManager(helper, this.Monitor);
            var energy = new EnergyManager(helper, this.Monitor);
            var greenhouse = new GreenhouseManager(helper, this.Monitor, energy);
            var outdoor = new OutdoorManager(helper, this.Monitor);
            var tv = new TVManager(helper, this.Monitor);

            this.Monitor.Log("FrostHaven mod loaded!", LogLevel.Info);
        }
    }
}
