using Sandbox;
using Sandbox.Network;
using Basebound.GameLoop;

namespace Basebound.Debug;

/// <summary>
/// Debug console commands for spawning test bots.
/// </summary>
public static class BotDebugCommands
{
	[ConCmd("bb_spawn_bot")]
	public static void SpawnBot(string botName = "Test Bot")
	{
		if (!Networking.IsHost)
		{
			Log.Warning("bb_spawn_bot: run this on the host/server.");
			return;
		}

		var botManager = BotManager.Instance;
		if (!botManager.IsValid())
		{
			Log.Warning("bb_spawn_bot: BotManager missing. Add it to the scene.");
			return;
		}

		botManager.AddBot(botName);
	}
}
