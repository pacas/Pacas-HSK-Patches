using Enlist;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VoidEvents
{
    public class VoidEnlistWorker : FactionEnlistWorker
	{
		public VoidEnlistWorker(FactionEnlistOptionsDef def) : base(def)
		{
		}

		public override bool CanEnlistTo(Faction toEnlist, out string cannotReason)
		{
			cannotReason = "";
			if (Current.Game.GetComponent<VoidGameComp>().enlistedAlready)
			{
				cannotReason = "Void.CannotEnlist".Translate();
				return false;
			}
			return true;
		}

        public override void EnlistTo(Faction toEnlist)
		{
			Find.WindowStack.Add(new Window_Password(new Dictionary<int, System.Action> 
			{
				{VoidMod.VoidEnlistPassword,	
					delegate
					{
						toEnlist.def.permanentEnemy = false;
						Current.Game.GetComponent<VoidGameComp>().enlistedAlready = true;
						base.EnlistTo(toEnlist);
                    }
				}
			}, "Void.BunkerPasswordMessage", "Void.AccessDenied"));
		}
	}
}