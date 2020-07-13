using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Respawning;
using Respawning.NamingRules;
using UnityEngine;

namespace EXILED.Patches
{
	[HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
	public class TeamRespawnEvent
	{
		public static bool Prefix(RespawnManager __instance)
		{
			if (EventPlugin.RespawnPatchDisable)
				return true;

      try
      {
        SpawnableTeam spawnableTeam;
        if (!RespawnWaveGenerator.SpawnableTeams.TryGetValue(__instance.NextKnownTeam, out spawnableTeam) ||
            __instance.NextKnownTeam == SpawnableTeamType.None)
        {
          ServerConsole.AddLog("Fatal error. Team '" + __instance.NextKnownTeam + "' is undefined.", ConsoleColor.Red);
        }
        else
        {
          List<ReferenceHub> list = ReferenceHub.GetAllHubs().Values.Where(item =>
            item.characterClassManager.CurClass == RoleType.Spectator && !item.serverRoles.OverwatchEnabled).ToList();
          if (__instance._prioritySpawn)
            list = list.OrderBy(item => item.characterClassManager.DeathTime).ToList();
          else
            list.ShuffleList();
          RespawnTickets singleton = RespawnTickets.Singleton;
          int a = singleton.GetAvailableTickets(__instance.NextKnownTeam);
          if (a == 0)
          {
            a = singleton.DefaultTeamAmount;
            RespawnTickets.Singleton.GrantTickets(singleton.DefaultTeam, singleton.DefaultTeamAmount, true);
          }

          bool isChaos = __instance.NextKnownTeam == SpawnableTeamType.ChaosInsurgency;
          int maxRespawn = Mathf.Min(a, spawnableTeam.MaxWaveSize);

          List<ReferenceHub> playersToRespawn = EventPlugin.DeadPlayers.Take(maxRespawn).ToList();
          Events.InvokeTeamRespawn(ref isChaos, ref maxRespawn, ref playersToRespawn);

          __instance.NextKnownTeam = isChaos ? SpawnableTeamType.ChaosInsurgency : SpawnableTeamType.NineTailedFox;

          if (playersToRespawn.Count > maxRespawn)
            playersToRespawn = playersToRespawn.Take(maxRespawn).ToList();
          list = playersToRespawn;

          int num = Mathf.Min(a, spawnableTeam.MaxWaveSize);
          while (list.Count > num)
            list.RemoveAt(list.Count - 1);
          list.ShuffleList();
          List<ReferenceHub> referenceHubList = ListPool<ReferenceHub>.Rent();
          foreach (ReferenceHub me in list)
          {
            try
            {
              RoleType classid =
                spawnableTeam.ClassQueue[Mathf.Min(referenceHubList.Count, spawnableTeam.ClassQueue.Length - 1)];
              me.characterClassManager.SetPlayersClass(classid, me.gameObject);
              referenceHubList.Add(me);
              ServerLogs.AddLog(ServerLogs.Modules.ClassChange,
                "Player " + me.LoggedNameFromRefHub() + " respawned as " + classid + ".",
                ServerLogs.ServerLogType.GameEvent);
            }
            catch (Exception ex)
            {
              if (me != null)
                ServerLogs.AddLog(ServerLogs.Modules.ClassChange,
                  "Player " + me.LoggedNameFromRefHub() + " couldn't be spawned. Err msg: " + ex.Message,
                  ServerLogs.ServerLogType.GameEvent);
              else
                ServerLogs.AddLog(ServerLogs.Modules.ClassChange,
                  "Couldn't spawn a player - target's ReferenceHub is null.", ServerLogs.ServerLogType.GameEvent);
            }
          }

          if (referenceHubList.Count > 0)
          {
            ServerLogs.AddLog(ServerLogs.Modules.ClassChange,
              "RespawnManager has successfully spawned " + referenceHubList.Count + " players as " +
              __instance.NextKnownTeam + "!", ServerLogs.ServerLogType.GameEvent);
            RespawnTickets.Singleton.GrantTickets(__instance.NextKnownTeam,
              -referenceHubList.Count * spawnableTeam.TicketRespawnCost);
            UnitNamingRule rule;
            if (UnitNamingRules.TryGetNamingRule(__instance.NextKnownTeam, out rule))
            {
              string regular;
              rule.GenerateNew(__instance.NextKnownTeam, out regular);
              foreach (ReferenceHub referenceHub in referenceHubList)
              {
                referenceHub.characterClassManager.NetworkCurSpawnableTeamType = (byte) __instance.NextKnownTeam;
                referenceHub.characterClassManager.NetworkCurUnitName = regular;
              }

              rule.PlayEntranceAnnouncement(regular);
            }

            RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn,
              __instance.NextKnownTeam);
          }

          __instance.NextKnownTeam = SpawnableTeamType.None;
        }
        
        return false;
      }
      catch (Exception exception)
      {
        Log.Error($"RespawnEvent error: {exception}");
        return true;
      }
    }
	}
}