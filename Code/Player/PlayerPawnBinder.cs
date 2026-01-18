using Sandbox;
using Sandbox.Network;

namespace Basebound.Player;

/// <summary>
/// Binds a spawned pawn to its owning PlayerClient on network spawn.
/// This keeps PlayerClient.Pawn valid for HUD and debug commands.
/// </summary>
public sealed class PlayerPawnBinder : Component, Component.INetworkSpawn
{
	void Component.INetworkSpawn.OnNetworkSpawn(Connection owner)
	{
		var client = FindClient(owner);
		if (!client.IsValid())
		{
			Log.Warning("PlayerPawnBinder: no PlayerClient found for pawn spawn.");
			return;
		}

		if (client.Pawn.IsValid() && client.Pawn.GameObject == GameObject)
			return;

		var pawnComponent = GameObject.Components.Get<PlayerController>()
			?? GameObject.Components.Get<Component>();

		if (!pawnComponent.IsValid())
		{
			Log.Warning("PlayerPawnBinder: pawn has no components to possess.");
			return;
		}

		PlayerClient.OnPossess(client, pawnComponent);
	}

	private PlayerClient FindClient(Connection owner)
	{
		var clients = Scene.GetAllComponents<PlayerClient>();
		if (owner is not null)
		{
			return clients.FirstOrDefault(candidate => candidate.IsValid() && candidate.Connection == owner);
		}

		if (PlayerClient.Local.IsValid())
			return PlayerClient.Local;

		return clients.FirstOrDefault(candidate => candidate.IsValid() && !candidate.IsProxy);
	}
}
