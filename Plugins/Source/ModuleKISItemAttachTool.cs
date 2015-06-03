using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace KIS
{

    public class ModuleKISItemAttachTool : ModuleKISItem
    {
        [KSPField]
        public float attachMaxMass = 0.5f;
        [KSPField]
        public bool allowStack = false;
        [KSPField]
        public bool attachModeIsWeld = false;
        [KSPField]
        public string attachSndPath = "KIS/Sounds/attach";
        [KSPField]
        public string detachSndPath = "KIS/Sounds/detach";
        [KSPField]
        public string changeModeSndPath = "KIS/Sounds/click";

        private bool orgDetachModeIsWeld;
        private string orgAttachSndPath, orgDetachSndPath;
        private float orgAttachMaxMass;

        public override string GetInfo()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<b>Maximum mass</b>: {0:F0}", attachMaxMass); sb.AppendLine();
            if (allowStack)
            {
                sb.AppendLine("Allow snap attach on stack node");
            }
            return sb.ToString();
        }

        public override void OnItemUse(KIS_Item item, KIS_Item.UseFrom useFrom)
        {
            // Check if grab key is pressed
            if (useFrom == KIS_Item.UseFrom.KeyDown)
            {
                if (!KISAddonPickup.draggedPart && !KISAddonPickup.instance.grabActive && !KISAddonPointer.isRunning)
                {
                    item.PlaySound(KIS_Shared.bipWrongSndPath);
                    ScreenMessages.PostScreenMessage("Use this tool while in drop mode to attach / Use grab key to detach", 5, ScreenMessageStyle.UPPER_CENTER);
                }
                if (KISAddonPointer.isRunning && KISAddonPointer.pointerTarget != KISAddonPointer.PointerTarget.PartMount)
                {
                    float attachPartMass = KISAddonPointer.partToAttach.mass + KISAddonPointer.partToAttach.GetResourceMass();
                    if (attachPartMass < attachMaxMass)
                    {
                        //test if the tool can fix this part (screw or weld)
                        bool testIfCanAttachPart = true;
                        if (KISAddonPointer.partToAttach.Modules.Contains("ModuleKISPartAttachMode"))
                        {
                            ModuleFlightAttachMode mkpam = (KISAddonPointer.partToAttach.Modules["ModuleKISPartAttachMode"] as ModuleFlightAttachMode);
                            if (!mkpam.canBeWeld && !mkpam.canBeScrewed)
                            {
                                ScreenMessages.PostScreenMessage("This part can't be attached", 5, ScreenMessageStyle.UPPER_CENTER);
                                testIfCanAttachPart = false;
                            }
                            else
                            {
                                testIfCanAttachPart = attachModeIsWeld ? mkpam.canBeWeld : mkpam.canBeScrewed;
                                item.PlaySound(KIS_Shared.bipWrongSndPath);
                                if (!testIfCanAttachPart)
                                {
                                    ScreenMessages.PostScreenMessage("This part can't be attached with this tool: it need a " +
                                        (attachModeIsWeld ? "screwdriver" : "weld tool"), 5, ScreenMessageStyle.UPPER_CENTER);
                                }
                            }
                        }
                        if (testIfCanAttachPart)
                        {
                            KISAddonPickup.instance.pointerMode = KISAddonPickup.PointerMode.Attach;
                            KISAddonPointer.allowStack = allowStack;
                            KISAddonPointer.toolAttachModeIsWeld = attachModeIsWeld;
                            item.PlaySound(changeModeSndPath);
                        }
                    }
                    else
                    {
                        item.PlaySound(KIS_Shared.bipWrongSndPath);
                        ScreenMessages.PostScreenMessage("This part is too heavy for this tool", 5, ScreenMessageStyle.UPPER_CENTER);
                    }
                }

            }
            if (useFrom == KIS_Item.UseFrom.KeyUp)
            {
                if (KISAddonPointer.isRunning && KISAddonPickup.instance.pointerMode == KISAddonPickup.PointerMode.Attach)
                {
                    KISAddonPointer.toolAttachModeIsWeld = attachModeIsWeld;
                    KISAddonPickup.instance.pointerMode = KISAddonPickup.PointerMode.Drop;
                    KISAddonPointer.allowStack = false;
                    item.PlaySound(changeModeSndPath);
                }
            }

        }

        public override void OnEquip(KIS_Item item)
        {
            ModuleKISPickup pickupModule = item.inventory.part.GetComponent<ModuleKISPickup>();
            if (pickupModule)
            {
                pickupModule.canDetach = true;
                orgDetachModeIsWeld = pickupModule.detachModeIsWeld;
                pickupModule.detachModeIsWeld = attachModeIsWeld;
                orgAttachMaxMass = pickupModule.detachMaxMass;
                pickupModule.detachMaxMass = attachMaxMass;
                orgAttachSndPath = pickupModule.attachSndPath;
                pickupModule.attachSndPath = attachSndPath;
                orgDetachSndPath = pickupModule.detachSndPath;
                pickupModule.detachSndPath = detachSndPath;
            }
        }

        public override void OnUnEquip(KIS_Item item)
        {
            ModuleKISPickup pickupModule = item.inventory.part.GetComponent<ModuleKISPickup>();
            if (pickupModule)
            {
                pickupModule.canDetach = false;
                pickupModule.detachModeIsWeld = orgDetachModeIsWeld;
                pickupModule.detachMaxMass = orgAttachMaxMass;
                pickupModule.attachSndPath = orgAttachSndPath;
                pickupModule.detachSndPath = orgDetachSndPath;
            }
        }

    }
}