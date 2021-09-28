using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI._ConsoleUI.Overtips;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
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

            modEntry.OnGUI = OnGUI;
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

        
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label(string.Format("Maximum distance to show info: <b>{0:F0}m</b>", settings.MaxDistance), new GUILayoutOption[0]);
            settings.MaxDistance = GUILayout.HorizontalSlider(settings.MaxDistance, 0f, 100f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
        }
        
        [HarmonyPatch(typeof(EntityOvertipVM), nameof(EntityOvertipVM.HighlightChanged))]
        static class EntityOvertipVM_HighlightChanged_Patch
        {
            static void Prefix(EntityOvertipVM __instance)
            {
                if (!enabled || !__instance.ObjectIsHovered.Value || !__instance.MapObject?.Get<InteractionLootPart>())
                    return;

                InteractionLootPart interactionLootPart = __instance.MapObject.Get<InteractionLootPart>();

                string text = !StringUtility.IsNullOrInvisible(interactionLootPart.GetName()) ? interactionLootPart.GetName() : UIStrings.Instance.LootWindow.GetLootName(interactionLootPart.Settings.LootContainerType);
                bool nearby = CharacterNearby(interactionLootPart);

                //Dbgl($"highlighted {text}; current text {__instance.Name.Value}, nearby {nearby}");

                if (__instance.Name.Value == text && nearby)
                {
                    var entries = interactionLootPart.Loot;
                    //Dbgl($"object has {entries.Count()} loots");
                    foreach (var entry in entries)
                    {
                        text += "\n"+entry.Name;
                        //Dbgl($"Added item {entry.Name} to name");
                    }
                    __instance.Name.Value = text;
                    //Dbgl($"Added items to name {text}");
                }
                else if (__instance.Name.Value != text && !nearby)
                {
                    __instance.Name.Value = text;
                    //Dbgl($"removed items from name {text}");
                }
            }
        }
        
        private static bool CharacterNearby(InteractionLootPart interactionLootPart)
        {
            if (settings.MaxDistance <= 0)
                return true;

            foreach(var c in Game.Instance.Player.Party)
            {
                //Dbgl($"check if {c.CharacterName} is close enough: {interactionLootPart.Owner.Position} {c.Position} {Vector3.Distance(interactionLootPart.Owner.Position, c.Position)}");
                if (Vector3.Distance(interactionLootPart.Owner.Position, c.Position) <= settings.MaxDistance)
                    return true;
            }
            return false;
        }
        
    }
}
