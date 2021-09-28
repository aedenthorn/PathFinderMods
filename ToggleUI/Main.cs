using HarmonyLib;
using Kingmaker.Cheats;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.Tooltip;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace ToggleUI
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
            modEntry.OnUpdate = OnUpdate;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (AedenthornUtils.CheckKeyDown(settings.ToggleKey))
            {
                Dbgl("Toggling HUD");
                AccessTools.Method(typeof(CheatsCommon), "ToggleHUD").Invoke(null, new object[] { "" });
            }
            /*
            else if (!settings.TooltipOnHold && AedenthornUtils.CheckKeyDown(settings.TooltipToggleKey))
            {
                settings.ShowTooltips = !settings.ShowTooltips;
                var tcs = Object.FindObjectsOfType<TooltipConstructor>();
                Dbgl($"Show {tcs.Length} Tooltips: "+settings.ShowTooltips);
                foreach (var tc in tcs)
                {
                    bool show = (bool)AccessTools.Field(typeof(TooltipBase), "m_NeedTooltip").GetValue(tc);
                    if(show != settings.ShowTooltips)
                    {
                        if (settings.ShowTooltips)
                            tc.Show(tc.CurrentTooltipData);
                        else
                            tc.Show(null);
                    }
                }
            }
            else if (settings.TooltipOnHold)
            {
                var tcs = Object.FindObjectsOfType<TooltipConstructor>();
                
                if(AedenthornUtils.CheckKeyDown(settings.TooltipToggleKey))
                    Dbgl($"Showing {tcs.Length} Tooltips");
                else if(AedenthornUtils.CheckKeyUp(settings.TooltipToggleKey))
                    Dbgl($"Hiding {tcs.Length} Tooltips");

                foreach (var tc in tcs)
                {
                    Object.Destroy(tc.gameObject);
                    continue;
                    bool show = (bool)AccessTools.Field(typeof(TooltipBase), "m_NeedTooltip").GetValue(tc);
                    if (show != AedenthornUtils.CheckKeyDown(settings.TooltipToggleKey))
                    {
                        if (AedenthornUtils.CheckKeyDown(settings.TooltipToggleKey))
                            tc.Show(tc.CurrentTooltipData);
                        else
                            tc.Show(null);
                    }
                }
            }
            */
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("UI Toggle Key: ", new GUILayoutOption[0]);
            settings.ToggleKey = GUILayout.TextField(settings.ToggleKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            /*
            GUILayout.BeginHorizontal();
            GUILayout.Label("Tooltip Toggle Key: ", new GUILayoutOption[0]);
            settings.TooltipToggleKey = GUILayout.TextField(settings.TooltipToggleKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            settings.TooltipOnHold = GUILayout.Toggle(settings.TooltipOnHold, "Only show tooltips while toggle is pressed", new GUILayoutOption[0]);
            GUILayout.Space(10f);

            settings.ShowTooltips = GUILayout.Toggle(settings.ShowTooltips, "Show tooltips (also changed by toggle key)", new GUILayoutOption[0]);
            GUILayout.Space(10f);
            */
        }

    }
}
