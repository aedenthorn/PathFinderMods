using HarmonyLib;
using Kingmaker.QA;
using Kingmaker.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace AdvancedDeveloperMode
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;

        private static readonly bool isDebug = true;
        
        private static List<string> commands = new List<string>();


        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Main).Namespace + " " : "") + str);
        }

        private static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ShowErrorWindow = GUILayout.Toggle(settings.ShowErrorWindow, "Show popup on game error", new GUILayoutOption[0]);
            GUILayout.Space(10f);

        }


        [HarmonyPatch(typeof(BuildModeUtility), nameof(BuildModeUtility.IsDevelopment))]
        [HarmonyPatch(MethodType.Getter)]
        static class BuildModeUtility_IsDevelopment_Getter_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(QAModeExceptionReporter), "ShowError")]
        static class QAModeExceptionReporter_ShowError_Patch
        {
            static bool Prefix()
            {
                return !enabled || settings.ShowErrorWindow;
            }
        }

    }
}
