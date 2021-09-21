using UnityModManagerNet;

namespace AdvancedDeveloperMode
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool ShowErrorWindow { get; set; } = false;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}