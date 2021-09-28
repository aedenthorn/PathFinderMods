using DG.Tweening;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Settings;
using Kingmaker.View;
using System.Reflection;
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
            GUILayout.Label("Reset Modifier Key: ", new GUILayoutOption[0]);
            settings.ResetModKey = GUILayout.TextField(settings.ResetModKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Reset Key: ", new GUILayoutOption[0]);
            settings.ResetKey = GUILayout.TextField(settings.ResetKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);


            GUILayout.Label(string.Format("Quick Zoom Speed Mult: <b>{0:F0}x</b>", settings.QuickZoomSpeed), new GUILayoutOption[0]);
            settings.QuickZoomSpeed = GUILayout.HorizontalSlider(settings.QuickZoomSpeed, 1f, 10f, new GUILayoutOption[0]);
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Quick Zoom Modifier Key: ", new GUILayoutOption[0]);
            settings.QuickZoomModKey = GUILayout.TextField(settings.QuickZoomModKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Elevation Modifier Key: ", new GUILayoutOption[0]);
            settings.ElevationModKey = GUILayout.TextField(settings.ElevationModKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Elevate Up Key: ", new GUILayoutOption[0]);
            settings.ElevateUpKey = GUILayout.TextField(settings.ElevateUpKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Elevate Down Key: ", new GUILayoutOption[0]);
            settings.ElevateDownKey = GUILayout.TextField(settings.ElevateDownKey, new GUILayoutOption[] { GUILayout.Width(100) });
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

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

            GUILayout.Label(string.Format("Mouse Rotation Speed Mult: <b>{0:F1}x</b>", settings.MouseRotationSpeed), new GUILayoutOption[0]);
            settings.MouseRotationSpeed = GUILayout.HorizontalSlider(settings.MouseRotationSpeed, 1f, 100f, new GUILayoutOption[0]) / 10f;
            GUILayout.Space(20f);

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
        
        public static float elevated = 0;
        public static float defaultElevation = 0;
        
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
                    Dbgl($"angles {__instance.transform.rotation.eulerAngles}");
                    
                    if(AedenthornUtils.CheckKeyHeld(settings.ZRotateModKey))
                        eulerAngles.z += ___m_RotationRatio * SettingsRoot.Controls.CameraRotationSpeedKeyboard * ___m_RotationSpeed * CameraRig.ConsoleRotationMod * (AedenthornUtils.CheckKeyHeld(settings.RotateLeftKey) ? -1 : 1);
                    else
                        eulerAngles.x += ___m_RotationRatio * SettingsRoot.Controls.CameraRotationSpeedKeyboard * ___m_RotationSpeed * CameraRig.ConsoleRotationMod * (AedenthornUtils.CheckKeyHeld(settings.RotateLeftKey) ? -1 : 1);
                    __instance.transform.DOKill(false);
                    __instance.transform.DOLocalRotate(eulerAngles, ___m_RotationTime, RotateMode.Fast).SetUpdate(true);
                }
            }
        }

        private static Vector3 lastMousePosition;

        [HarmonyPatch(typeof(CameraRig), "RotateByMiddleButton")]
        static class CameraRig_RotateByMiddleButton_Patch
        {
            static bool Prefix(CameraRig __instance, float ___m_RotationRatio, float ___m_RotationTime)
            {
                if (!enabled || !Input.GetMouseButton(2) || (!AedenthornUtils.CheckKeyHeld(settings.XRotateModKey) && !AedenthornUtils.CheckKeyHeld(settings.ZRotateModKey)))
                {
                    lastMousePosition = -Vector3.one;
                    return true;
                }

                if(lastMousePosition.x >= 0)
                {
                    float rotation = Input.mousePosition.x - lastMousePosition.x;
                    Vector3 eulerAngles = __instance.transform.rotation.eulerAngles;
                    if (AedenthornUtils.CheckKeyHeld(settings.ZRotateModKey))
                        eulerAngles.z += rotation * settings.MouseRotationSpeed;
                    else
                        eulerAngles.x += rotation * settings.MouseRotationSpeed;
                    __instance.transform.DOKill(false);
                    __instance.transform.DOLocalRotate(eulerAngles, ___m_RotationTime, RotateMode.Fast).SetUpdate(true);
                }
                lastMousePosition = Input.mousePosition;
                return false;
            }
        }

        [HarmonyPatch(typeof(CameraRig), nameof(CameraRig.TickScroll))]
        static class CameraRig_TickScroll_Patch
        {
            static void Postfix(CameraRig __instance, ref Vector3 ___m_TargetPosition)
            {
                if (!enabled)
                    return;

                if(elevated > 0)
                    ___m_TargetPosition = new Vector3(___m_TargetPosition.x, elevated, ___m_TargetPosition.z);
            }
        }
        [HarmonyPatch(typeof(CameraRig), "Update")]
        static class CameraRig_Update_Patch
        {
            static void Prefix(CameraRig __instance, ref Vector3 ___m_TargetPosition)
            {
                if (!enabled)
                    return;
                if (AedenthornUtils.CheckKeyHeld(settings.ResetModKey) && AedenthornUtils.CheckKeyDown(settings.ResetKey))
                {
                    __instance.transform.eulerAngles = new Vector3(0, __instance.transform.rotation.eulerAngles.y, 0);
                    if(defaultElevation > 0)
                    {
                        elevated = defaultElevation;
                    }
                }
                else if (AedenthornUtils.CheckKeyHeld(settings.ElevationModKey) && (AedenthornUtils.CheckKeyHeld(settings.ElevateUpKey) || AedenthornUtils.CheckKeyHeld(settings.ElevateDownKey)))
                {
                    if (elevated == 0)
                        defaultElevation = ___m_TargetPosition.y;

                    float e = 0.1f * (AedenthornUtils.CheckKeyHeld(settings.ElevateUpKey) ? 1 : -1);
                    ___m_TargetPosition += new Vector3(0, e, 0);
                    if (___m_TargetPosition.y <= 0)
                        ___m_TargetPosition = new Vector3(___m_TargetPosition.x, 0.01f, ___m_TargetPosition.z);


                    elevated = ___m_TargetPosition.y;
                }
            }
        }
    }
}
