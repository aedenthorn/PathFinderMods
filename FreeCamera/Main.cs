using DG.Tweening;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Controllers.Rest;
using Kingmaker.Settings;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityModManagerNet;

namespace FreeCamera
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Quick Zoom Modifier Key: ", new GUILayoutOption[0]);
            settings.QuickZoomModKey = GUILayout.TextField(settings.QuickZoomModKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("X-Axis Modifier Key: ", new GUILayoutOption[0]);
            settings.XRotateModKey = GUILayout.TextField(settings.XRotateModKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Z-Axis Modifier Key: ", new GUILayoutOption[0]);
            settings.ZRotateModKey = GUILayout.TextField(settings.ZRotateModKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotate Left Key: ", new GUILayoutOption[0]);
            settings.RotateLeftKey = GUILayout.TextField(settings.RotateLeftKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotate Right Key: ", new GUILayoutOption[0]);
            settings.RotateRightKey = GUILayout.TextField(settings.RotateRightKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.Label(string.Format("Quick Zoom Speed Mult: <b>{0:F0}x</b>", settings.QuickZoomSpeed), new GUILayoutOption[0]);
            settings.QuickZoomSpeed = GUILayout.HorizontalSlider(settings.QuickZoomSpeed, 1f, 10f, new GUILayoutOption[0]);
            GUILayout.Space(10f);
        }


        [HarmonyPatch(typeof(CameraZoom), nameof(CameraZoom.TickZoom))]
        static class CameraZoom_TickZoom_Patch
        {
            static bool Prefix(CameraZoom __instance, Coroutine ___m_ZoomRoutine, float ___m_Smooth, ref float ___m_PlayerScrollPosition, ref float ___m_ScrollPosition, ref float ___m_SmoothScrollPosition, Camera ___m_Camera, float ___m_ZoomLenght)
            {
                if (!enabled)
                    return true;


                if (___m_ZoomRoutine != null)
                {
                    return true;
                }


                if (!__instance.IsScrollBusy && Game.Instance.IsControllerMouse && Input.GetAxis("Mouse ScrollWheel") != 0 && (___m_Camera.fieldOfView > settings.FOVMin || Input.GetAxis("Mouse ScrollWheel") < 0))
                {
                    Dbgl($"mouse {Input.GetAxis("Mouse ScrollWheel")} {___m_Camera.fieldOfView} {settings.FOVMin}");
                    ___m_PlayerScrollPosition += (__instance.IsOutOfScreen ? 0f : Input.GetAxis("Mouse ScrollWheel") * (AedenthornUtils.CheckKeyHeld(settings.QuickZoomModKey) ? settings.QuickZoomSpeed : 1));

                    Dbgl($"scroll {___m_PlayerScrollPosition}");

                    if (___m_PlayerScrollPosition <= 0)
                        ___m_PlayerScrollPosition = 0.01f;
                }

                if (___m_PlayerScrollPosition <= 0)
                {
                    ___m_PlayerScrollPosition = (settings.FOVMax - settings.FOVMin) / 18f;
                    Dbgl($"scroll {___m_PlayerScrollPosition}");

                }

                ___m_ScrollPosition = ___m_PlayerScrollPosition;
                ___m_SmoothScrollPosition = Mathf.Lerp(___m_SmoothScrollPosition, ___m_PlayerScrollPosition, Time.unscaledDeltaTime * ___m_Smooth);
                ___m_Camera.fieldOfView = Mathf.Lerp(settings.FOVMax, settings.FOVMin, __instance.CurrentNormalizePosition * (__instance.FovMax - __instance.FovMin) / (settings.FOVMax - settings.FOVMin));
                ___m_PlayerScrollPosition = ___m_ScrollPosition;

                return false;
            }
        }
        
        [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.TickRotate))]
        static class CameraRig_TickRotate_Patch
        {
            static void Postfix(CameraRig __instance, float ___m_RotationSpeed, float ___m_RotationRatio, float ___m_RotationTime)
            {
                if (!enabled)
                    return;

                if((AedenthornUtils.CheckKeyHeld(settings.XRotateModKey) || AedenthornUtils.CheckKeyHeld(settings.ZRotateModKey)) && (AedenthornUtils.CheckKeyHeld(settings.RotateLeftKey) || AedenthornUtils.CheckKeyHeld(settings.RotateRightKey)))
                {
                    Vector3 eulerAngles = __instance.transform.rotation.eulerAngles;
                    
                    if(AedenthornUtils.CheckKeyHeld(settings.ZRotateModKey))
                        eulerAngles.z += ___m_RotationRatio * SettingsRoot.Controls.CameraRotationSpeedKeyboard * ___m_RotationSpeed * CameraRig.ConsoleRotationMod * (AedenthornUtils.CheckKeyHeld(settings.RotateLeftKey) ? -1 : 1);
                    else
                        eulerAngles.x += ___m_RotationRatio * SettingsRoot.Controls.CameraRotationSpeedKeyboard * ___m_RotationSpeed * CameraRig.ConsoleRotationMod * (AedenthornUtils.CheckKeyHeld(settings.RotateLeftKey) ? -1 : 1);

                    __instance.transform.DOKill(false);
                    __instance.transform.DOLocalRotate(eulerAngles, ___m_RotationTime, RotateMode.Fast).SetUpdate(true);
                }

            }
        }

        private static float Clamp(float value, float min, float max)
        {
            return value;
        }
    }
}
