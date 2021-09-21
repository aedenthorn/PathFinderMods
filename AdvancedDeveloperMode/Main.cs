using HarmonyLib;
using Kingmaker.Cheats;
using Kingmaker.QA;
using Kingmaker.Settings;
using Kingmaker.Utility;
using System;
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
        
        [HarmonyPatch(typeof(CheatsCommon), nameof(CheatsCommon.RegisterCheats))]
        static class CheatsCommon_RegisterCheats_Patch
        {
            static void Postfix()
            {
                SmartConsole.RegisterCommand("CanSeeTheirClassSpecificClothes", new SmartConsole.ConsoleCommandFunction(ToggleCanSeeTheirClassSpecificClothes));
                SmartConsole.RegisterCommand("DressAllCompanionsInDrobyshevskysShirt", new SmartConsole.ConsoleCommandFunction(ToggleDressAllCompanionsInDrobyshevskysShirt));
                SmartConsole.RegisterCommand("FeedCritters", new SmartConsole.ConsoleCommandFunction(ToggleFeedCritters));
                SmartConsole.RegisterCommand("InsteadOfBloodSprinkleRandomCritters", new SmartConsole.ConsoleCommandFunction(ToggleInsteadOfBloodSprinkleRandomCritters));
                SmartConsole.RegisterCommand("SpawnOwlcatOnGlobalmap", new SmartConsole.ConsoleCommandFunction(ToggleSpawnOwlcatOnGlobalmap));
            }
        }

        private static void ToggleCanSeeTheirClassSpecificClothes(string parameters)
        {
            SettingsRoot.Game.SillyCheatCodes.CanSeeTheirClassSpecificClothes.SetValueAndConfirm(!SettingsRoot.Game.SillyCheatCodes.CanSeeTheirClassSpecificClothes.GetValue());
            SmartConsole.WriteLine($"Set CanSeeTheirClassSpecificClothes to {SettingsRoot.Game.SillyCheatCodes.CanSeeTheirClassSpecificClothes.GetValue()}");
            Dbgl($"Set CanSeeTheirClassSpecificClothes to {SettingsRoot.Game.SillyCheatCodes.CanSeeTheirClassSpecificClothes.GetValue()}");
        }

        private static void ToggleDressAllCompanionsInDrobyshevskysShirt(string parameters)
        {
            SettingsRoot.Game.SillyCheatCodes.DressAllCompanionsInDrobyshevskysShirt.SetValueAndConfirm(!SettingsRoot.Game.SillyCheatCodes.DressAllCompanionsInDrobyshevskysShirt.GetValue());
            SmartConsole.WriteLine($"Set DressAllCompanionsInDrobyshevskysShirt to {SettingsRoot.Game.SillyCheatCodes.DressAllCompanionsInDrobyshevskysShirt.GetValue()}");
            Dbgl($"Set DressAllCompanionsInDrobyshevskysShirt to {SettingsRoot.Game.SillyCheatCodes.DressAllCompanionsInDrobyshevskysShirt.GetValue()}");
        }

        private static void ToggleFeedCritters(string parameters)
        {
            SettingsRoot.Game.SillyCheatCodes.FeedCritters.SetValueAndConfirm(!SettingsRoot.Game.SillyCheatCodes.FeedCritters.GetValue());
            SmartConsole.WriteLine($"Set FeedCritters to {SettingsRoot.Game.SillyCheatCodes.FeedCritters.GetValue()}");
            Dbgl($"Set FeedCritters to {SettingsRoot.Game.SillyCheatCodes.FeedCritters.GetValue()}");
        }

        private static void ToggleInsteadOfBloodSprinkleRandomCritters(string parameters)
        {
            SettingsRoot.Game.SillyCheatCodes.InsteadOfBloodSprinkleRandomCritters.SetValueAndConfirm(!SettingsRoot.Game.SillyCheatCodes.InsteadOfBloodSprinkleRandomCritters.GetValue());
            SmartConsole.WriteLine($"Set InsteadOfBloodSprinkleRandomCritters to {SettingsRoot.Game.SillyCheatCodes.InsteadOfBloodSprinkleRandomCritters.GetValue()}");
            Dbgl($"Set InsteadOfBloodSprinkleRandomCritters to {SettingsRoot.Game.SillyCheatCodes.InsteadOfBloodSprinkleRandomCritters.GetValue()}");
        }

        private static void ToggleSpawnOwlcatOnGlobalmap(string parameters)
        {
            SettingsRoot.Game.SillyCheatCodes.SpawnOwlcatOnGlobalmap.SetValueAndConfirm(!SettingsRoot.Game.SillyCheatCodes.SpawnOwlcatOnGlobalmap.GetValue());
            SmartConsole.WriteLine($"Set SpawnOwlcatOnGlobalmap to {SettingsRoot.Game.SillyCheatCodes.SpawnOwlcatOnGlobalmap.GetValue()}");
            Dbgl($"Set SpawnOwlcatOnGlobalmap to {SettingsRoot.Game.SillyCheatCodes.SpawnOwlcatOnGlobalmap.GetValue()}");
        }
    }
}
