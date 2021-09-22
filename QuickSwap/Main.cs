using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UI.MVVM;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Inventory;
using Kingmaker.UI.MVVM._VM.ServiceWindows;
using Kingmaker.UI.MVVM._VM.ServiceWindows.Inventory;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Sound;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityModManagerNet;

namespace QuickSwap
{
    public class Main
    {
        public static Settings settings { get; private set; }

        public static bool enabled;
        private static KeyCode[] keyCodes;
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
            modEntry.OnUpdate = OnUpdate;

            keyCodes = new KeyCode[6] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6 };
            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (!enabled)
                return;

            int idx = -1;

            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    idx = i;
                    break;
                }
            }
            if (idx > -1)
            {
                var party = Game.Instance.Player.Party;

                Dbgl($"pressed {keyCodes[idx]}; party size {party.Count}");

                if (party.Count > idx)
                {
                    var which = party[idx];

                    if(RootUIContext.Instance.CurrentServiceWindow == ServiceWindowsType.Inventory)
                    {
                        PointerEventData eventData = new PointerEventData(EventSystem.current)
                        {
                            position = Input.mousePosition
                        };
                        List<RaycastResult> raycastResults = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(eventData, raycastResults);

                        foreach (RaycastResult rcr in raycastResults)
                        {
                            if (rcr.gameObject.layer == LayerMask.NameToLayer("UI") && rcr.gameObject.GetComponent<InventorySlotView>() && rcr.gameObject.GetComponent<InventorySlotView>().Item != null)
                            {
                                Dbgl("Got item in slot");

                                var inventory = RootUIContext.Instance.InGameVM.StaticPartVM.ServiceWindowsVM.InventoryVM.Value;

                                var item = rcr.gameObject.GetComponent<InventorySlotView>().Item;

                                bool success = false;

                                Dbgl("Checking if weapon or shield");

                                if (item is ItemEntityWeapon || item is ItemEntityShield)
                                {
                                    int handSlot = 0;
                                    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
                                        handSlot = 3;
                                    else if (Input.GetKey(KeyCode.LeftShift))
                                        handSlot = 1;
                                    else if (Input.GetKey(KeyCode.LeftControl))
                                        handSlot = 2;

                                    Dbgl($"trying to equip {item.Name} on {which.CharacterName} in slot {handSlot}");

                                    if (item is ItemEntityWeapon)
                                        success = TryEquipSlotWeapon(item as ItemEntityWeapon, which.Descriptor, which.Descriptor.Body.HandsEquipmentSets[handSlot]);
                                    else
                                        success = TryEquipShield(item, which.Descriptor, which.Descriptor.Body.HandsEquipmentSets[handSlot]);
                                }
                                else
                                {
                                    Dbgl($"trying to equip {item.Name} on {which.CharacterName}");
                                    success = UIUtilityItem.TryEquipItemAutomaticaly(item, which.Descriptor);
                                }

                                if (success)
                                {
                                    Dbgl($"equipped {item.Name} on {which.CharacterName}");
                                    UISoundController.Instance.PlayItemSound(SlotAction.Put, item, true);
                                    inventory.Refresh();
                                    return;
                                }
                            }
                        }
                    }

                    if (RootUIContext.Instance.CurrentServiceWindow == ServiceWindowsType.None && Input.GetKey(KeyCode.LeftShift))
                    {
                        if (Game.Instance.UI.SelectionManager.SelectedUnits.Contains(which))
                        {
                            Dbgl($"trying to remove {which.CharacterName} from selected characters");
                            Game.Instance.UI.SelectionManager.UnselectUnit(which);
                        }
                        else
                        {
                            Dbgl($"trying to add {which.CharacterName} to selected characters");
                            Game.Instance.UI.SelectionManager.SelectUnit(which.View, false);
                        }
                    }
                    else if(RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.None || Input.GetKey(KeyCode.LeftControl))
                    {
                        Dbgl($"trying to switch to {which.CharacterName}");
                        Game.Instance.SelectionCharacter.SetSelected(which);
                    }
                }
            }
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

        private static bool TryEquipSlotWeapon(ItemEntityWeapon weapon, UnitDescriptor unit, HandsEquipmentSet handsEquipmentSet)
        {
            if (weapon == null || unit == null)
            {
                return false;
            }
            ItemEntityWeapon itemEntityWeapon = handsEquipmentSet.PrimaryHand.MaybeItem as ItemEntityWeapon;
            HandSlot primaryHand = handsEquipmentSet.PrimaryHand;
            HandSlot secondaryHand = handsEquipmentSet.SecondaryHand;
            HandSlot targetSlot;
            if (weapon.Blueprint.IsTwoHanded || (itemEntityWeapon != null && itemEntityWeapon.Blueprint.IsTwoHanded))
            {
                targetSlot = primaryHand;
            }
            else if (!primaryHand.HasItem)
            {
                targetSlot = primaryHand;
            }
            else if (!secondaryHand.HasItem)
            {
                targetSlot = secondaryHand;
            }
            else
            {
                targetSlot = primaryHand;
            }
            return TryInsertItemTo(weapon, targetSlot);
        }

        private static bool TryEquipShield(ItemEntity shield, UnitDescriptor unit, HandsEquipmentSet handsEquipmentSet)
        {
            if (shield == null || unit == null)
            {
                return false;
            }
            HandSlot targetSlot = (handsEquipmentSet != null) ? handsEquipmentSet.SecondaryHand : null;
            return TryInsertItemTo(shield, targetSlot);
        }
        private static bool TryInsertItemTo(ItemEntity item, Kingmaker.Items.Slots.ItemSlot targetSlot)
        {
            if (targetSlot == null || !targetSlot.CanInsertItem(item) || (targetSlot.HasItem && !targetSlot.CanRemoveItem()))
            {
                if (targetSlot != null)
                {
                    UnitDescriptor owner = targetSlot.Owner;
                    if (owner != null)
                    {
                        UnitEntityData unit = owner.Unit;
                        if (unit != null)
                        {
                            UnitEntityView view = unit.View;
                            if (view != null)
                            {
                                UnitAsksComponent asks = view.Asks;
                                if (asks != null)
                                {
                                    asks.RefuseEquip.Schedule();
                                }
                            }
                        }
                    }
                }
                return false;
            }
            targetSlot.InsertItem(item);
            return true;
        }
    }
}
