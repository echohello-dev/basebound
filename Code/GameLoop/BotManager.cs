using Sandbox;
using Sandbox.Network;
using Basebound.Bots;
using Basebound.Player;

namespace Basebound.GameLoop;

/// <summary>
/// Manages debug-only bot spawning for the vertical slice.
/// </summary>
public sealed class BotManager : Component
{
	private static BotManager _instance;

	public static BotManager Instance => _instance.IsValid() ? _instance : null;

	[Property, Title("Client Prefab")]
	public GameObject ClientPrefab { get; set; }

	[Property, Title("Pawn Prefab")]
	public GameObject PawnPrefab { get; set; }

	[Property, Title("Bot Count")]
	public int BotCount { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();

		if (!Networking.IsHost)
			return;

		if (_instance.IsValid() && _instance != this)
		{
			Log.Warning("BotManager: multiple instances detected; using first instance.");
			return;
		}

		_instance = this;
	}

	public void AddBot(string botName = "Test Bot")
	{
		if (!Networking.IsHost)
			return;

		if (!ClientPrefab.IsValid() || !PawnPrefab.IsValid())
		{
			Log.Warning("BotManager: assign ClientPrefab and PawnPrefab in the scene.");
			return;
		}

		var clientObject = ClientPrefab.Clone();
		clientObject.Name = $"client_bot_{BotCount}";

		var pawnObject = PawnPrefab.Clone();
		pawnObject.Name = $"pawn_bot_{BotCount}";

		var playerClient = clientObject.Components.Get<Basebound.Player.PlayerClient>();
		if (!playerClient.IsValid())
		{
			Log.Warning("BotManager: PlayerClient missing on ClientPrefab.");
			clientObject.Destroy();
			pawnObject.Destroy();
			return;
		}

		playerClient.IsBot = true;
		playerClient.DisplayName = botName;

		var botController = clientObject.Components.GetOrCreate<BotController>();
		botController.BotName = botName;

		var pawnComponent = pawnObject.Components.Get<WeaponComponent>() ?? pawnObject.Components.Get<Component>();
		if (!pawnComponent.IsValid())
		{
			Log.Warning("BotManager: pawn prefab has no components to possess.");
			clientObject.Destroy();
			pawnObject.Destroy();
			return;
		}

		Basebound.Player.PlayerClient.OnPossess(playerClient, pawnComponent);

		if (!clientObject.Network.Active)
			clientObject.NetworkSpawn(Connection.Host);
		else
			clientObject.Network.AssignOwnership(Connection.Host);

		if (!pawnObject.Network.Active)
			pawnObject.NetworkSpawn(Connection.Host);

		BotCount++;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (_instance == this)
			_instance = null;
	}
}
