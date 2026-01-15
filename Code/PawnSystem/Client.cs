using Basebound.Player;
using Sandbox;

namespace Basebound.PawnSystem;

/// <summary>
/// Networked representation of a connected player.
/// Mirrors the Facepunch hc1 pattern with Local/Viewer statics so UI and gameplay
/// systems have a single source of truth for "who are we" information.
/// </summary>
public sealed class Client : Component
{
	/// <summary>
	/// The client whose perspective we are currently rendering on this machine.
	/// </summary>
	public static Client Viewer { get; private set; }

	/// <summary>
	/// The client object owned by this machine (if any).
	/// </summary>
	public static Client Local { get; private set; }

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
	public PlayerBase PlayerState { get; private set; }

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
	public bool IsLocalPlayer => !IsProxy && Connection == Connection.Local;

	/// <summary>
	/// Are we currently viewing this client?
	/// </summary>
	public bool IsViewer => Viewer == this;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureIdentityMetadata();
		TryAssignStatics();
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
	public static void OnPossess(Client client, Component pawn)
	{
		if (!client.IsValid())
		{
			Log.Warning("Attempted to possess with an invalid client.");
			return;
		}

		client.Pawn = pawn;

		if (!pawn.IsValid())
		{
			return;
		}

		if (client.IsLocalPlayer)
		{
			Local = client;
			Viewer = client;
		}
		else if (Viewer == client)
		{
			Viewer = client;
		}
	}

	/// <summary>
	/// Helper for the local machine to switch viewer context (spectating, cameras, etc.).
	/// </summary>
	public static void SetViewer(Client client)
	{
		if (client.IsValid())
		{
			Viewer = client;
		}
	}

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

			if (!Viewer.IsValid())
			{
				Viewer = this;
			}
		}
		else if (!Viewer.IsValid())
		{
			Viewer = this;
		}
	}
}
