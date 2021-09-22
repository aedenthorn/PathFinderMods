using UnityModManagerNet;

namespace QuickSwap
{
    public class Settings : UnityModManager.ModSettings
    {
        //public float MaxDistance { get; set; } = 1;
        
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}