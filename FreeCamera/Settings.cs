using UnityModManagerNet;

namespace FreeCamera
{
    public class Settings : UnityModManager.ModSettings
    {
        public string XRotateModKey { get; set; } = "left shift";
        public string ZRotateModKey { get; set; } = "left ctrl";
        public string RotateLeftKey { get; set; } = "q";
        public string RotateRightKey { get; set; } = "e";
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}