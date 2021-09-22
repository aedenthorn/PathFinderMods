using UnityModManagerNet;

namespace LootPreview
{
    public class Settings : UnityModManager.ModSettings
    {
        public float MaxDistance { get; set; } = 5;
        
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}