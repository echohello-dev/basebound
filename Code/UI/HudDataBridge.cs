// Intentionally blank.
//
// `HudDataBridge` lives in `Code/UI/HUD/HudDataBridge.cs`.
// This file previously contained a duplicate definition and is kept only to avoid
// breaking any existing file references or editor history.
using Basebound.Player;
using Sandbox;
using System;

namespace Basebound.UI;

/// <summary>
/// Bridge component that samples game state and exposes HUD-friendly projections.
/// Razor panels read from this bridge instead of reaching into game objects directly.
/// 
/// This pattern provides:
/// - Single point of null handling
/// - Clean separation between game logic and UI
/// - Easy debugging (log bridge values to verify data flow)
/// - Spectate-ready (always reads from Viewer, not Local)
/// </summary>
public sealed class HudDataBridge : Component
{
	// ===== VIEWER REFERENCES =====

	/// <summary>
	/// The client we're currently viewing (local player or spectate target).
	/// </summary>
	private PlayerClient ViewerPlayerClient => PlayerClient.Viewer;

	/// <summary>
	/// The player state of who we're viewing.
	/// </summary>
	private PlayerState ViewerPlayerState => ViewerPlayerClient?.PlayerState;

	// ===== VIEWER INFO =====

	/// <summary>
	/// Whether we have a valid viewer to display data for.
	/// </summary>
	public bool HasViewer => ViewerPlayerClient.IsValid() && ViewerPlayerState.IsValid();

	/// <summary>
	/// Display name of the current viewer.
	/// </summary>
	public string ViewerName => ViewerPlayerClient?.DisplayName ?? "Unknown";

	/// <summary>
	/// Whether we're spectating someone else (not our own client).
	/// </summary>
	public bool IsSpectating => false;

	// ===== HEALTH =====

	/// <summary>
	/// Current health as a percentage (0-100).
	/// </summary>
	public float HealthPercent
	{
		get
		{
			var pct01 = ViewerPlayerState.IsValid() ? ViewerPlayerState.HealthPercent : 0f;
			pct01 = Math.Clamp(pct01, 0f, 1f);
			return pct01 * 100f;
		}
	}

	/// <summary>
	/// Current health value.
	/// </summary>
	public int CurrentHealth => ViewerPlayerState?.CurrentHealth ?? 0;
	/// <summary>
	/// Maximum health value.
	/// </summary>
	public int MaxHealth => ViewerPlayerState?.MaxHealth ?? 100;

	/// <summary>
	/// Whether health is critically low (≤25%).
	/// </summary>
	public bool IsCriticalHealth => ViewerPlayerState?.IsCriticalHealth ?? false;
	/// <summary>
	/// Whether the viewer is alive.
	/// </summary>
	public bool IsAlive => ViewerPlayerState?.IsAlive ?? false;

	// ===== ECONOMY =====

	/// <summary>
	/// Current currency balance.
	/// </summary>
	public long Currency => ViewerPlayerState?.Currency ?? 0;

	/// <summary>
	/// Formatted currency string (e.g., "1,000").
	/// </summary>
	public string CurrencyFormatted => ViewerPlayerState?.CurrencyFormatted ?? "$0";

	/// <summary>
	/// Passive income rate formatted (e.g., "+5/s").
	/// </summary>
	public string PassiveIncomeFormatted => ViewerPlayerState?.PassiveIncomeFormatted ?? "+0/s";

	// ===== AMMO (equipped weapon source for Slice 2) =====

	private Basebound.Player.WeaponComponent ViewerWeapon => ViewerPlayerClient?.Pawn?.GameObject?.Components?.Get<Basebound.Player.WeaponComponent>();

	/// <summary>
	/// Weapon display name.
	/// </summary>
	public string WeaponName => ViewerWeapon?.WeaponName ?? "Weapon";

	/// <summary>
	/// Ammo type label (e.g., 5.56, 9mm).
	/// </summary>
	public string AmmoType => ViewerWeapon?.AmmoType ?? "Ammo";

	/// <summary>
	/// Current firing mode label (e.g., Auto, Burst, Semi).
	/// </summary>
	public string FiringMode => ViewerWeapon?.FiringMode ?? "Auto";

	/// <summary>
	/// Current ammo in magazine.
	/// </summary>
	public int CurrentAmmo => ViewerWeapon?.CurrentAmmo ?? 0;

	/// <summary>
	/// Maximum ammo capacity.
	/// </summary>
	public int MaxAmmo => ViewerWeapon?.MaxAmmo ?? 0;

	/// <summary>
	/// Reserve ammo count.
	/// </summary>
	public int ReserveAmmo => ViewerWeapon?.ReserveAmmo ?? 0;

	/// <summary>
	/// Whether the current weapon has ammo.
	/// </summary>
	public bool HasAmmo => CurrentAmmo > 0;



	// ===== ROUND TIMER (placeholder for timer slice) =====

	/// <summary>
	/// Round timer as formatted string (e.g., "1:30").
	/// </summary>
	public string TimerText => "0:00"; // TODO: Slice 3 - read from RoundTimerGlobal

	/// <summary>
	/// Remaining seconds in round.
	/// </summary>
	public float TimerSeconds => 0f; // TODO: Slice 3 - read from RoundTimerGlobal

	// ===== PROGRESSION =====

	/// <summary>
	/// Player's current level.
	/// </summary>
	public int PlayerLevel => ViewerPlayerState?.PlayerLevel ?? 1;

	/// <summary>
	/// Contracts completed count.
	/// </summary>
	public int ContractsCompleted => ViewerPlayerState?.ContractsCompleted ?? 0;

	/// <summary>
	/// Current skill level.
	/// </summary>
	public float SkillLevel => ViewerPlayerState?.SkillLevel ?? 1f;

	// ===== DEBUG =====

	/// <summary>
	/// Debug string showing current viewer state.
	/// </summary>
	public string DebugInfo => $"Local: {PlayerClient.Local.IsValid()}, Viewer: {ViewerPlayerClient.IsValid()}, Player: {ViewerPlayerState.IsValid()}, Health: {HealthPercent:F0}%";
}
