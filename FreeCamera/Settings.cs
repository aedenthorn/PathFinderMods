using UnityModManagerNet;

namespace FreeCamera
{
    public class Settings : UnityModManager.ModSettings
    {
        public string QuickZoomModKey { get; set; } = "left shift";
        public string ElevationModKey { get; set; } = "left ctrl";
        public string ElevateUpKey { get; set; } = "w";
        public string ElevateDownKey { get; set; } = "s";
        public float QuickZoomSpeed { get; set; } = 2;
        public string XRotateModKey { get; set; } = "left shift";
        public string ZRotateModKey { get; set; } = "left ctrl";
        public string RotateLeftKey { get; set; } = "q";
        public string RotateRightKey { get; set; } = "e";
        public float FOVMax { get; set; } = 90;
        public float FOVMin { get; set; } = 1;
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}