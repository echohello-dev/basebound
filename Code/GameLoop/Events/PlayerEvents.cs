using System;

namespace Basebound.GameLoop.Events;

/// <summary>
/// Lightweight event hub for early vertical slices.
///
/// Note: This is intentionally simple (static events) until we pull in a more
/// robust event system. UI can subscribe to these events without reaching into
/// gameplay objects.
/// </summary>
public static class GameEvents
{
	public static event Action<PlayerDamagedEvent> PlayerDamaged;
	public static event Action<PlayerKilledEvent> PlayerKilled;

	public static void Raise(PlayerDamagedEvent e) => PlayerDamaged?.Invoke(e);
	public static void Raise(PlayerKilledEvent e) => PlayerKilled?.Invoke(e);
}

/// <summary>
/// Raised when a player takes damage (host-authoritative).
/// </summary>
public record PlayerDamagedEvent(
	ulong VictimSteamId,
	string VictimName,
	ulong AttackerSteamId,
	string AttackerName,
	int Damage,
	string Method
);

/// <summary>
/// Raised when a player is killed (host-authoritative).
/// </summary>
public record PlayerKilledEvent(
	ulong VictimSteamId,
	string VictimName,
	ulong KillerSteamId,
	string KillerName,
	string Method
);
