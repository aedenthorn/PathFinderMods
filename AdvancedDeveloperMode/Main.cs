using HarmonyLib;
using Kingmaker;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
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
                SmartConsole.RegisterCommand("list_companions", new SmartConsole.ConsoleCommandFunction(ListCompanions));
                SmartConsole.RegisterCommand("list_banters", new SmartConsole.ConsoleCommandFunction(ListBanter));
                SmartConsole.RegisterCommand("list_locs", new SmartConsole.ConsoleCommandFunction(ListLocs));
                SmartConsole.RegisterCommand("romance_info", new SmartConsole.ConsoleCommandFunction(RomanceInfo));
                SmartConsole.RegisterCommand("romance_increase", "romance_increase CamelliaRomanceRomanceCounter", new SmartConsole.ConsoleCommandFunction(RomanceIncrease));
                SmartConsole.RegisterCommand("romance_unlock", "romance_unlock CamelliaRomanceRomanceCounter", new SmartConsole.ConsoleCommandFunction(RomanceUnlock));
            }
        }

        private static void ListLocs(string parameters)
        {
            string paramString = Utilities.GetParamString(parameters, 1, null);
            foreach (BlueprintAreaEnterPoint blueprintAreaEnterPoint in Utilities.GetScriptableObjects<BlueprintAreaEnterPoint>())
            {
                try
                {
                    if (paramString == null || blueprintAreaEnterPoint.name.Contains(paramString))
                    {
                        PFLog.SmartConsole.Log(blueprintAreaEnterPoint.name + " LocalizedName: " + blueprintAreaEnterPoint.Area.AreaName, Array.Empty<object>());
                    }
                }
                catch(Exception ex)
                {
                    Dbgl($"Exception getting loc {blueprintAreaEnterPoint?.name}:\n\n{ex}");
                }
            }
        }

        private static void ListCompanions(string parameters)
        {
            string value = Utilities.GetParamString(parameters, 1, null) ?? "";
            foreach (BlueprintUnit blueprint in Utilities.GetScriptableObjects<BlueprintUnit>())
            {
                string blueprintPath = Utilities.GetBlueprintPath(blueprint);
                if (blueprintPath.Contains(value))
                {
                    PFLog.SmartConsole.Log(blueprintPath, Array.Empty<object>());
                }
            }
        }
        
        private static void ListBanter(string parameters)
        {
            string value = Utilities.GetParamString(parameters, 1, null) ?? "";
            foreach (BlueprintBarkBanter blueprint in Utilities.GetScriptableObjects<BlueprintBarkBanter>())
            {
                string blueprintPath = Utilities.GetBlueprintPath(blueprint);
                if (blueprintPath.Contains(value))
                {
                    PFLog.SmartConsole.Log(blueprintPath, Array.Empty<object>());
                }
            }
        }

        private static void RomanceInfo(string parameters)
        {
            foreach (BlueprintRomanceCounter blueprintRomanceCounter in Utilities.GetScriptableObjects<BlueprintRomanceCounter>())
            {
                PFLog.SmartConsole.Log("Romance " + Utilities.GetBlueprintName(blueprintRomanceCounter), Array.Empty<object>());
                PFLog.SmartConsole.Log(FlagInfo(blueprintRomanceCounter.CounterFlag), Array.Empty<object>());
                PFLog.SmartConsole.Log(FlagInfo(blueprintRomanceCounter.MaxValueFlag), Array.Empty<object>());
                PFLog.SmartConsole.Log(FlagInfo(blueprintRomanceCounter.CounterFlag), Array.Empty<object>());
            }
        }
        
        private static void RomanceIncrease(string parameters)
        {
            foreach (BlueprintRomanceCounter blueprintRomanceCounter in Utilities.GetScriptableObjects<BlueprintRomanceCounter>())
            {
                try
                {
                    string paramString = Utilities.GetParamString(parameters, 1, "Missing parameter");
                    if (Utilities.GetBlueprintName(blueprintRomanceCounter) == paramString)
                    {
                        blueprintRomanceCounter.UnlockFlags();
                        if (blueprintRomanceCounter.MaxValueFlag.Value <= blueprintRomanceCounter.CounterFlag.Value)
                            blueprintRomanceCounter.MaxValueFlag.Value++;
                        blueprintRomanceCounter.CounterFlag.Value++;
                        PFLog.SmartConsole.Log($"Romance increased from {(blueprintRomanceCounter.CounterFlag.Value - 1)} to {blueprintRomanceCounter.CounterFlag.Value}", Array.Empty<object>());
                    }
                }
                catch { }
            }
        }
                
        private static void RomanceUnlock(string parameters)
        {
            foreach (BlueprintRomanceCounter blueprintRomanceCounter in Utilities.GetScriptableObjects<BlueprintRomanceCounter>())
            {
                try { 
                    string paramString = Utilities.GetParamString(parameters, 1, "Missing parameter");
                    if (Utilities.GetBlueprintName(blueprintRomanceCounter) == paramString)
                    {
                        blueprintRomanceCounter.UnlockFlags();
                        PFLog.SmartConsole.Log("Romance unlocked", Array.Empty<object>());
                    }
                }
                catch { }

            }
        }

        private static string FlagInfo(BlueprintUnlockableFlag counterCounterFlag)
        {
            if (counterCounterFlag == null)
                return "";

            Dictionary<BlueprintUnlockableFlag, int> unlockedFlags = Game.Instance.Player.UnlockableFlags.UnlockedFlags;
            if (unlockedFlags.ContainsKey(counterCounterFlag))
            {
                return Utilities.GetBlueprintName(counterCounterFlag) + unlockedFlags[counterCounterFlag];
            }
            return Utilities.GetBlueprintName(counterCounterFlag) + " Absent";
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
