using Sandbox;

namespace Basebound.Player;

/// <summary>
/// Temporary equipped-weapon component for Slice 2 ammo telemetry.
/// Replace with real weapon system when available.
/// </summary>
public sealed class WeaponComponent : Component
{
	private TimeSince _timeSinceLastShot = 0f;

	[Property, Header("Weapon Info"), Title("Weapon Name")]
	public string WeaponName { get; set; } = "Carbine";


	[Property, Title("Ammo Type")]
	public string AmmoType { get; set; } = "5.56";

	[Property, Title("Firing Mode")]
	public string FiringMode { get; set; } = "Auto";

	[Property, Header("Ammo"), Range(1, 500), Title("Magazine Size")]
	public int MaxAmmo { get; set; } = 30;

	[Property, ReadOnly, Range(0, 500), Title("Current Ammo")]
	[Sync] public int CurrentAmmo { get; set; } = 30;

	[Property, ReadOnly, Range(0, 999), Title("Reserve Ammo")]
	[Sync] public int ReserveAmmo { get; set; } = 90;

	[Property, Header("Input"), Title("Auto Fire")]
	public bool AutoFire { get; set; } = true;

	[Property, Range(0.05f, 1f), Title("Shots Per Second")]
	public float FireRate { get; set; } = 8f;


	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (IsProxy)
			return;

		HandleFireInput();
		HandleReloadInput();
	}

	/// <summary>
	/// Attempts to consume ammo from the magazine.
	/// </summary>
	public bool ConsumeAmmo(int amount = 1)
	{
		if (amount <= 0 || CurrentAmmo < amount)
			return false;

		CurrentAmmo -= amount;
		CurrentAmmo = System.Math.Max(CurrentAmmo, 0);
		return true;
	}

	/// <summary>
	/// Reloads the magazine from reserve ammo.
	/// </summary>
	public void ReloadAmmo()
	{
		if (MaxAmmo <= 0)
			return;

		var missing = MaxAmmo - CurrentAmmo;
		if (missing <= 0 || ReserveAmmo <= 0)
			return;

		var toLoad = System.Math.Min(missing, ReserveAmmo);
		CurrentAmmo += toLoad;
		ReserveAmmo -= toLoad;
	}

	private void HandleFireInput()
	{
		var canFire = AutoFire ? Input.Down("Attack1") : Input.Pressed("Attack1");
		if (!canFire)
			return;

		if (FireRate <= 0f)
			return;

		var timeBetweenShots = 1f / FireRate;
		if (_timeSinceLastShot < timeBetweenShots)
			return;

		if (!ConsumeAmmo())
			return;

		_timeSinceLastShot = 0f;
	}

	private void HandleReloadInput()
	{
		if (!Input.Pressed("Reload"))
			return;

		ReloadAmmo();
	}
}

