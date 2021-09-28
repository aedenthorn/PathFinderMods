using UnityModManagerNet;

namespace ToggleUI
{
    public class Settings : UnityModManager.ModSettings
    {
        public string ToggleKey { get; set; } = "home";
        /*
        public string TooltipToggleKey { get; set; } = "end";
        public bool TooltipOnHold { get; set; } = false;
        public bool ShowTooltips { get; set; } = true;
        */
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}