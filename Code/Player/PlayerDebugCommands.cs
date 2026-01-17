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

		if (!player.ConsumeAmmo(amount))
		{
			Log.Warning($"bb_consume_ammo: not enough ammo (Current {player.CurrentAmmo}).");
			return;
		}

		Log.Info($"bb_consume_ammo: {amount} -> {player.CurrentAmmo}/{player.MaxAmmo} (Reserve {player.ReserveAmmo})");
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

		player.ReloadAmmo();
		Log.Info($"bb_reload: -> {player.CurrentAmmo}/{player.MaxAmmo} (Reserve {player.ReserveAmmo})");
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

		player.MaxAmmo = max;
		player.CurrentAmmo = current;
		player.ReserveAmmo = reserve;
		Log.Info($"bb_set_ammo: -> {player.CurrentAmmo}/{player.MaxAmmo} (Reserve {player.ReserveAmmo})");
	}
}
