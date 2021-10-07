using HarmonyLib;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace RemoveGenderReq
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
        }
        [HarmonyPatch(typeof(CompanionStoriesManager), nameof(CompanionStoriesManager.Get), new Type[] { typeof(UnitEntityData) })]
        static class CompanionStoriesManager_Get_Patch
        {
            static bool Prefix(CompanionStoriesManager __instance, UnitEntityData character, ref IEnumerable<BlueprintCompanionStory> __result)
            {
                if (!enabled)
                    return true;
                if (!BlueprintRoot.Instance.CharGen.CustomCompanions.Any((BlueprintUnitReference r) => r.Is(character.Blueprint)))
                {
                    return true;
                }
                __result = from r in BlueprintRoot.Instance.CharGen.CustomCompanionStories
                       select r.Get() into st
                       where !st.IsDlcRestricted()
                       select st;

                return false;
            }
        }
        [HarmonyPatch(typeof(ContextConditionGender), "CheckCondition")]
        static class ContextConditionGender_Check_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;

                __result = true;

                return false;
            }
        }
        [HarmonyPatch(typeof(UnitGender), "CheckCondition")]
        static class UnitGender_Check_Patch
        {
            static bool Prefix(ref bool __result)
            {
                if (!enabled)
                    return true;

                __result = true;

                return false;
            }
        }
    }
}
