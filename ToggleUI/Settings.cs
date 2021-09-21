using UnityModManagerNet;

namespace ToggleUI
{
    public class Settings : UnityModManager.ModSettings
    {
        public string ToggleKey { get; set; } = "home";
        
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}