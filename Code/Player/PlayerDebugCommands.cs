using Sandbox;

namespace Basebound.Player;

/// <summary>
/// Small set of console commands to make Slice 1 (health HUD) testable without needing
/// fall damage / weapons yet.
///
/// Notes:
/// - Health is host-authoritative in <see cref="PlayerState"/> (it early-outs when <c>IsProxy</c>).
/// - These commands therefore only mutate health when executed on the host/listen server.
/// </summary>
public static class PlayerDebugCommands
{
	private static PlayerState GetLocalPlayerState()
	{
		var client = PlayerClient.Local;
		if (!client.IsValid())
			return null;

		var player = client.PlayerState;
		return player.IsValid() ? player : null;
	}

	private static WeaponComponent GetLocalWeapon()
	{
		var client = PlayerClient.Local;
		if (!client.IsValid())
			return null;

		var pawn = client.Pawn;
		if (!pawn.IsValid())
			return null;

		var weapon = pawn.GameObject.Components.Get<WeaponComponent>();
		return weapon.IsValid() ? weapon : null;
	}


	[ConCmd("bb_damage")]
	public static void Damage(int amount = 10)
	{
		amount = amount.Clamp(0, 10000);

		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_damage: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_damage: PlayerState is a proxy. Run this on the host/listen server to actually change health.");
			return;
		}

		player.TakeDamage(amount);
		Log.Info($"bb_damage: {amount} -> {player.CurrentHealth}/{player.MaxHealth}");
	}

	[ConCmd("bb_heal")]
	public static void Heal(int amount = 10)
	{
		amount = amount.Clamp(0, 10000);

		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_heal: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_heal: PlayerState is a proxy. Run this on the host/listen server to actually change health.");
			return;
		}

		player.Heal(amount);
		Log.Info($"bb_heal: {amount} -> {player.CurrentHealth}/{player.MaxHealth}");
	}

	[ConCmd("bb_kill")]
	public static void Kill()
	{
		Damage(10000);
	}

	[ConCmd("bb_revive")]
	public static void Revive()
	{
		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_revive: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_revive: PlayerState is a proxy. Run this on the host/listen server to actually change health.");
			return;
		}

		player.IsAlive = true;
		player.CurrentHealth = player.MaxHealth;
		Log.Info($"bb_revive: -> {player.CurrentHealth}/{player.MaxHealth}");
	}

	[ConCmd("bb_set_health")]
	public static void SetHealth(int health = 100)
	{
		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_set_health: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_set_health: PlayerState is a proxy. Run this on the host/listen server to actually change health.");
			return;
		}

		health = health.Clamp(0, player.MaxHealth);
		player.CurrentHealth = health;
		player.IsAlive = player.CurrentHealth > 0;

		Log.Info($"bb_set_health: -> {player.CurrentHealth}/{player.MaxHealth} (IsAlive={player.IsAlive})");
	}

	[ConCmd("bb_consume_ammo")]
	public static void ConsumeAmmo(int amount = 1)
	{
		amount = amount.Clamp(1, 999);

		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_consume_ammo: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_consume_ammo: PlayerState is a proxy. Run this on the host/listen server to actually change ammo.");
			return;
		}

		var weapon = GetLocalWeapon();
		if (weapon is null)
		{
			Log.Warning("bb_consume_ammo: no WeaponComponent found on pawn.");
			return;
		}

		if (!weapon.ConsumeAmmo(amount))
		{
			Log.Warning($"bb_consume_ammo: not enough ammo (Current {weapon.CurrentAmmo}).");
			return;
		}

		Log.Info($"bb_consume_ammo: {amount} -> {weapon.CurrentAmmo}/{weapon.MaxAmmo} (Reserve {weapon.ReserveAmmo})");
	}

	[ConCmd("bb_reload")]
	public static void ReloadAmmo()
	{
		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_reload: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_reload: PlayerState is a proxy. Run this on the host/listen server to actually change ammo.");
			return;
		}

		var weapon = GetLocalWeapon();
		if (weapon is null)
		{
			Log.Warning("bb_reload: no WeaponComponent found on pawn.");
			return;
		}

		weapon.ReloadAmmo();
		Log.Info($"bb_reload: -> {weapon.CurrentAmmo}/{weapon.MaxAmmo} (Reserve {weapon.ReserveAmmo})");
	}

	[ConCmd("bb_set_ammo")]
	public static void SetAmmo(int current = 30, int reserve = 90, int max = 30)
	{
		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_set_ammo: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_set_ammo: PlayerState is a proxy. Run this on the host/listen server to actually change ammo.");
			return;
		}

		max = max.Clamp(1, 999);
		current = current.Clamp(0, max);
		reserve = reserve.Clamp(0, 999);

		var weapon = GetLocalWeapon();
		if (weapon is null)
		{
			Log.Warning("bb_set_ammo: no WeaponComponent found on pawn.");
			return;
		}

		weapon.MaxAmmo = max;
		weapon.CurrentAmmo = current;
		weapon.ReserveAmmo = reserve;
		Log.Info($"bb_set_ammo: -> {weapon.CurrentAmmo}/{weapon.MaxAmmo} (Reserve {weapon.ReserveAmmo})");
	}

	[ConCmd("bb_add_weapon")]
	public static void AddWeapon()
	{
		var player = GetLocalPlayerState();
		if (player is null)
		{
			Log.Warning("bb_add_weapon: no local PlayerState found (PlayerClient.Local is null?)");
			return;
		}

		if (player.IsProxy)
		{
			Log.Warning("bb_add_weapon: PlayerState is a proxy. Run this on the host/listen server to attach weapon.");
			return;
		}

		var client = PlayerClient.Local.IsValid() ? PlayerClient.Local : PlayerClient.Viewer;
		if (!client.IsValid())
		{
			Log.Warning("bb_add_weapon: no local or viewer client found.");
			return;
		}

		var pawn = client.Pawn;
		if (!pawn.IsValid())
		{
			Log.Warning($"bb_add_weapon: {client.DisplayName} has no pawn yet.");
			return;
		}

		var pawnObject = pawn.GameObject;
		if (!pawnObject.IsValid())
		{
			Log.Warning("bb_add_weapon: pawn has no valid GameObject.");
			return;
		}

		var weapon = pawnObject.Components.GetOrCreate<WeaponComponent>();
		Log.Info($"bb_add_weapon: attached {weapon.WeaponName} ({weapon.CurrentAmmo}/{weapon.MaxAmmo}, Reserve {weapon.ReserveAmmo}).");
	}

	[ConCmd("bb_dump_clients")]
	public static void DumpClients()
	{
		var scene = Game.ActiveScene;
		if (!scene.IsValid())
		{
			Log.Warning("bb_dump_clients: no active scene.");
			return;
		}

		Log.Info($"bb_dump_clients: Local={(PlayerClient.Local.IsValid() ? PlayerClient.Local.DisplayName : "null")}, Viewer={(PlayerClient.Viewer.IsValid() ? PlayerClient.Viewer.DisplayName : "null")}");

		foreach (var client in scene.GetAllComponents<PlayerClient>())
		{
			if (!client.IsValid())
				continue;

			var connection = client.Connection;
			var connLabel = connection is null ? "null" : $"{connection.DisplayName} (Host={connection.IsHost}, Active={connection.IsActive})";
			Log.Info($"- Client {client.DisplayName} | Id={client.Id} | IsProxy={client.IsProxy} | IsLocalPlayer={client.IsLocalPlayer} | Conn={connLabel} | PawnValid={client.Pawn.IsValid()} | PlayerStateValid={client.PlayerState.IsValid()} | IsAlive={client.PlayerState?.IsAlive}");
		}
	}
}
