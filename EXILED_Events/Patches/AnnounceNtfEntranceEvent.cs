using Harmony;
using System;
using Respawning;
using Respawning.NamingRules;

namespace EXILED.Patches
{
	[HarmonyPatch(typeof(UnitNamingRule), nameof(UnitNamingRule.PlayEntranceAnnouncement))]
	public class AnnounceNtfEntranceEvent
	{
		public static bool Prefix(UnitNamingRule __instance, ref string regular)
		{
			try
			{
				bool allow = true;

				int scp_alive = 0;
				int unit_number = 0;
				char unit_letter = 'a';
				Events.InvokeAnnounceNtfEntrance(ref scp_alive, ref unit_number, ref unit_letter, ref allow);

				return allow;
			}
			catch (Exception exception)
			{
				Log.Error($"AnnounceNtfEntranceEvent error: {exception}");
				return true;
			}
		}
	}
}
