using MEC;
using Respawning;

namespace EXILED.Extensions
{
	public static class Cassie
	{
		/// <summary>
		/// Plays a cassie message.
		/// </summary>
		public static void CassieMessage(string msg, bool makeHold, bool makeNoise) => RespawnEffectsController.PlayCassieAnnouncement(msg, makeHold, makeNoise);

		/// <summary>
		/// Plays a cassie message with a delay.
		/// </summary>
		public static void DelayedCassieMessage(string msg, bool makeHold, bool makeNoise, float delay)
		{
			Timing.CallDelayed(delay, () => RespawnEffectsController.PlayCassieAnnouncement(msg, makeHold, makeNoise));
		}
	}
}
