using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cheats;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace LootPreview
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

            //modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnToggle = OnToggle;
            //modEntry.OnUpdate = OnUpdate;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {

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

        /*
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Maximum distance to show info: <b>{0:F0}x</b>", settings.MaxDistance), new GUILayoutOption[0]);
            settings.MaxDistance = GUILayout.HorizontalSlider(settings.MaxDistance, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
        }
        */

        [HarmonyPatch(typeof(InteractionLootPart), nameof(InteractionLootPart.GetName))]
        static class InteractionLootPart_GetName_Patch
        {
            static void Postfix(InteractionLootPart __instance, ref string __result)
            {
                if (!enabled || !__instance.Settings.AddMapMarker)
                    return;

                if (StringUtility.IsNullOrInvisible(__result))
                    __result = UIStrings.Instance.LootWindow.GetLootName(__instance.Settings.LootContainerType);

                var entries = __instance.Loot;
                if (entries.Any())
                {
                    foreach(var entry in entries)
                    {
                        __result += $"\n{entry.Name}";
                    }
                }
            }
        }
        /*
        private static bool CharacterNearby(InteractionLootPart instance)
        {
            foreach(var c in Game.Instance.Player.Party)
            {
                //Dbgl($"check if {c.CharacterName} is close enough: {instance.Owner.Position} {c.Position} {Vector3.Distance(instance.Owner.Position, c.Position)}");
                if (Vector3.Distance(instance.Owner.Position, c.Position) <= settings.MaxDistance)
                    return true;
            }
            return false;
        }
        */
    }
}
