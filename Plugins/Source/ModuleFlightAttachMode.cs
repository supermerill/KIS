using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;

namespace KIS
{

    /**
     * This class can be add to a part to add some info about rule for attach/detach (screw vs weld)
     * This name is defined as is instead of "ModuleKISAttachMode" to have a better info name.
     * */
    public class ModuleFlightAttachMode : PartModule
    {
        //used to know if this part is welded or screwed tohis parent (ignored if !canBeScrewed)
        // Maybe add onguiactive= true to show this information to the player.
        //default : screwed
        [KSPField(isPersistant=true)]
        public bool isWelded = false;

        //set true if this part can receive / can be attache via screws
        [KSPField]
        public bool canBeScrewed = false;

        //set true if this part can be weld 
        // if false and canBeScrewed false => it can't be attached/detach in flight 
        //        => part too complex to do that in flight (like for engine)
        [KSPField]
        public bool canBeWeld = true;

        //to show what can be done with it on the 
        public override string GetInfo()
        {
            string info = "";
            if(canBeScrewed)
            {
                info += "Can be screwed";
            }
            if (canBeWeld)
            {
                info += (info.Length>0?"\n":"")+"Can be weld";
            }
            if (info.Length == 0)
            {
                info += "Can't be attach in flight";
            }
            return info;
        }

		public override void OnStart(PartModule.StartState state)
		{
			base.OnStart(state);
			if (state == StartState.PreLaunch)
			{
				if (!this.canBeScrewed) this.isWelded = true;
				else if (part.parent.Modules.Contains("ModuleFlightAttachMode"))
				{
					ModuleFlightAttachMode parentMod = part.parent.Modules["ModuleFlightAttachMode"] as ModuleFlightAttachMode;
					if (parentMod.canBeScrewed)
					{
						//this piece in screwed in the vab if the parent and this can be screwed together.
						this.isWelded = false;
					}
				}
				else
				{
					//this part can be screwed and the parent has no information => ok for screwing.
					this.isWelded = false;
				}
			}
		}
    }

}