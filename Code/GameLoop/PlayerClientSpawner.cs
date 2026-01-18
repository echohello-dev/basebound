using Sandbox;
using Sandbox.Network;
using Basebound.Player;

namespace Basebound.GameLoop;

/// <summary>
/// Spawns PlayerClient objects when connections become active.
/// NetworkHelper remains responsible for starting the server and spawning pawns.
/// </summary>
public sealed class PlayerClientSpawner : Component, Component.INetworkListener
{
	[Property, Title("Client Prefab")]
	public GameObject ClientPrefab { get; set; }

	public void OnActive(Connection channel)
	{
		if (!Networking.IsHost)
			return;

		var existing = Scene.GetAllComponents<PlayerClient>()
			.FirstOrDefault(candidate => candidate.IsValid() && candidate.Connection == channel);
		if (existing.IsValid())
			return;

		if (!ClientPrefab.IsValid())
		{
			Log.Warning("PlayerClientSpawner: no ClientPrefab assigned.");
			return;
		}

		var clientObject = ClientPrefab.Clone();
		clientObject.Name = $"client_{channel.DisplayName}";

		var client = clientObject.Components.Get<PlayerClient>();
		if (!client.IsValid())
		{
			Log.Warning("PlayerClientSpawner: ClientPrefab missing PlayerClient.");
			clientObject.Destroy();
			return;
		}

		if (!clientObject.Network.Active)
			clientObject.NetworkSpawn(channel);
		else
			clientObject.Network.AssignOwnership(channel);
	}
}
