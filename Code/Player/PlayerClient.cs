using Sandbox;

namespace Basebound.Player;

/// <summary>
/// Networked representation of a connected player.
/// Mirrors the Facepunch hc1 pattern with Local/Viewer statics so UI and gameplay
/// systems have a single source of truth for "who are we" information.
/// </summary>
public sealed class PlayerClient : Component
{
	/// <summary>
	/// The client whose perspective we are currently rendering on this machine.
	/// </summary>
	public static PlayerClient Viewer { get; private set; }

	/// <summary>
	/// The client object owned by this machine (if any).
	/// </summary>
	public static PlayerClient Local { get; private set; }

	/// <summary>
	/// Steam identifier for this connection. Synced from the host.
	/// </summary>
	[Property, Sync(SyncFlags.FromHost)]
	public ulong SteamId { get; set; }

	/// <summary>
	/// Display name used for HUD / scoreboards. Synced from the host.
	/// </summary>
	[Property, Sync(SyncFlags.FromHost)]
	public string DisplayName { get; set; }

	/// <summary>
	/// Optional flag for bots to let UI distinguish them.
	/// </summary>
	[Property, Sync(SyncFlags.FromHost)]
	public bool IsBot { get; set; }

	/// <summary>
	/// Main gameplay state for this client.
	/// </summary>
	[RequireComponent]
	public PlayerState PlayerState { get; private set; }

	/// <summary>
	/// The currently possessed pawn (player character, drone, etc.).
	/// </summary>
	[Sync]
	public Component Pawn { get; private set; }

	/// <summary>
	/// Network connection that owns this client object.
	/// </summary>
	public Connection Connection => Network?.Owner;

	/// <summary>
	/// Indicates if the underlying connection is active or hosting.
	/// </summary>
	public bool IsConnected => Connection is not null && (Connection.IsActive || Connection.IsHost);

	/// <summary>
	/// True when this component belongs to the local machine.
	/// </summary>
	/// <remarks>
	/// In fully networked play this is a strict ownership check.
	/// In early vertical-slice scenes (or editor-only prototypes) a Client may exist without
	/// a network connection; in that case we treat the non-proxy instance as local so UI can function.
	/// </remarks>
	public bool IsLocalPlayer => !IsProxy && (Connection == Connection.Local || (Connection is null && !Local.IsValid()));

	/// <summary>
	/// Are we currently viewing this client?
	/// </summary>
	public bool IsViewer => Viewer == this;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureIdentityMetadata();
		TryAssignStatics();
		_wasAlive = PlayerState?.IsAlive ?? true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Viewer selection is a *local* concern. Proxies should never drive it.
		if (!IsLocalPlayer)
			return;

		// If viewer was cleared by a destroy on the spectate target, fall back to local.
		if (!Viewer.IsValid() && Local.IsValid())
		{
			Viewer = Local;
		}

		if (!PlayerState.IsValid())
			return;

		var isAliveNow = PlayerState.IsAlive;

		// Transition: alive -> dead
		if (_wasAlive && !isAliveNow)
		{
			// Spectating is intentionally disabled (gameplay: don't reveal bases/positions).
			// Keep the view context on the local client.
			Viewer = this;
		}
		// Transition: dead -> alive (respawn)
		else if (!_wasAlive && isAliveNow)
		{
			// Always return view to ourselves on respawn.
			Viewer = this;
		}

		_wasAlive = isAliveNow;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (Viewer == this)
		{
			Viewer = null;
		}

		if (Local == this)
		{
			Local = null;
		}
	}

	/// <summary>
	/// Called by the pawn/controller layer whenever possession changes.
	/// Updates local/client static references and ensures UI follows the current view target.
	/// </summary>
	public static void OnPossess(PlayerClient client, Component pawn)
	{
		if (!client.IsValid())
		{
			Log.Warning("Attempted to possess with an invalid client.");
			return;
		}

		client.Pawn = pawn;

		// Possession changes should never allow remote clients to hijack the viewer.
		// For the local machine, keep Local/Viewer pointed at our local client even if pawn is null
		// (death / transition / respawn).
		if (client.IsLocalPlayer)
		{
			Local = client;
			Viewer = client;
		}
	}

	/// <summary>
	/// Helper for the local machine to switch viewer context (spectating, cameras, etc.).
	/// </summary>
	public static void SetViewer(PlayerClient client)
	{
		// Spectating is intentionally disabled (gameplay: don't reveal bases/positions).
		// Force viewer to stay on the local client.
		if (Local.IsValid())
			Viewer = Local;
	}

	private bool _wasAlive = true;

	private void EnsureIdentityMetadata()
	{
		if (string.IsNullOrWhiteSpace(DisplayName))
		{
			DisplayName = Connection?.DisplayName ?? PlayerState?.PlayerName ?? $"Player {Id}";
		}

		if (SteamId == 0 && Connection is not null)
		{
			SteamId = Connection.SteamId;
		}
	}

	private void TryAssignStatics()
	{
		if (IsLocalPlayer)
		{
			Local = this;

			// Viewer is a clientside concept - default it to our local client.
			if (!Viewer.IsValid())
				Viewer = this;
		}
	}
}
