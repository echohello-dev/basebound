using Sandbox;
using Basebound.GameLoop.Events;
using BBClient = Basebound.Player.PlayerClient;

namespace Basebound.Player;

/// <summary>
/// Core player component managing identity, progression, and state.
/// Handles currency, health, skill tracking, and game system integration.
/// </summary>
public sealed class PlayerState : Component
{
	private BBClient OwnerClient => GameObject?.Components?.Get<BBClient>();

	private BBClient _lastDamageAttacker;
	private string _lastDamageMethod;
	private bool _deathDispatched;

	// ===== PLAYER IDENTITY =====
	[Property, Header("Player Identity"), Title("Name")]
	public string PlayerName { get; set; } = "Player";

	[Property, Range(1, 100), Description("Player's current level")]
	public int PlayerLevel { get; set; } = 1;

	// ===== ECONOMY SYSTEM =====
	[Property, Header("Economy System"), ReadOnly, Icon("payments")]
	[Sync] public long Currency { get; set; } = 1000;

	[Property, Range(1, 1000), Title("Currency Per Second"), Description("Passive income rate")]
	public long CurrencyPerTick { get; set; } = 5;

	[Property, Icon("gavel")]
	public bool IsTaxed { get; set; } = false;

	[Property, ReadOnly, Title("Lifetime Earnings"), Icon("trending_up")]
	[Sync] public long TotalEarnings { get; set; } = 0;

	// ===== HEALTH & STATUS =====
	[Property, Header("Health & Status"), Range(1, 1000), Icon("favorite")]
	public int MaxHealth { get; set; } = 100;

	[Property, ReadOnly, Range(0, 1000), Icon("favorite_border")]
	[Sync] public int CurrentHealth { get; set; } = 100;

	[Property, ReadOnly, Icon("health_and_safety")]
	[Sync] public bool IsAlive { get; set; } = true;


	// ===== PROGRESSION & SKILLS =====
	[Property, Header("Progression"), ReadOnly, Icon("assignment_turned_in")]
	[Sync] public int ContractsCompleted { get; set; } = 0;

	[Property, ReadOnly, Range(1f, 100f), Title("Skill Level")]
	[Sync] public float SkillLevel { get; set; } = 1f;

	// ===== RAID SYSTEM =====
	[Property, Header("Raid System"), ReadOnly, Icon("security")]
	public bool IsRaidingNow { get; set; } = false;

	[Property, ReadOnly, Icon("shield")]
	[Sync] public int RaidsInitiated { get; set; } = 0;

	[Property, ReadOnly, Icon("verified_user")]
	[Sync] public int RaidsDefended { get; set; } = 0;

	// ===== BASE BUILDING =====
	[Property, Header("Base Building"), ReadOnly, Icon("home")]
	[Sync] public bool HasBase { get; set; } = false;

	[Property, ReadOnly, Icon("grid_on")]
	[Sync] public int BlocksPlaced { get; set; } = 0;

	public float HealthPercent => MaxHealth == 0 ? 0f : (float)CurrentHealth / MaxHealth;
	public bool IsCriticalHealth => CurrentHealth <= MaxHealth * 0.25f;
	public string CurrencyFormatted => Currency.ToString("N0");
	public string PassiveIncomeFormatted => $"+{CurrencyPerTick}/s";


	// State tracking
	private float _currencyTickAccumulator = 0f;

	protected override void OnStart()
	{
		_deathDispatched = false;
		_lastDamageAttacker = null;
		_lastDamageMethod = null;

		// Validate health
		CurrentHealth = System.Math.Min(CurrentHealth, MaxHealth);

		Log.Info($"Player {PlayerName} joined! Level {PlayerLevel}, Balance: {Currency}");
	}

	protected override void OnUpdate()
	{
		// Only host processes player state
		if (IsProxy) return;

		// Validate alive state
		if (CurrentHealth <= 0 && IsAlive)
		{
			IsAlive = false;
			DispatchKilledEvent();
			BroadcastPlayerDeath();
		}
	}

	protected override void OnFixedUpdate()
	{
		// Only host processes passive economy
		if (IsProxy) return;

		ProcessPassiveEconomy();
	}

	/// <summary>
	/// Processes passive currency generation (50 ticks/sec).
	/// </summary>
	private void ProcessPassiveEconomy()
	{
		_currencyTickAccumulator += CurrencyPerTick / 50f; // Convert to per-tick

		if (_currencyTickAccumulator >= 1f)
		{
			long currencyGained = (long)_currencyTickAccumulator;
			AddCurrency(currencyGained);
			_currencyTickAccumulator -= currencyGained;
		}
	}

	/// <summary>
	/// Adds currency to player balance.
	/// </summary>
	public void AddCurrency(long amount)
	{
		if (amount < 0) return;

		Currency += amount;
		TotalEarnings += amount;
		BroadcastCurrencyUpdate();
	}


	/// <summary>
	/// Removes currency if player has sufficient balance.
	/// </summary>

	public bool RemoveCurrency(long amount)
	{
		if (amount < 0 || Currency < amount)
			return false;

		Currency -= amount;
		BroadcastCurrencyUpdate();
		return true;
	}

	/// <summary>
	/// Deals damage to the player.
	/// </summary>
	public void TakeDamage(int damage)
	{
		TakeDamage(damage, null, null);
	}

	/// <summary>
	/// Deals damage to the player, optionally providing an attacker and method.
	/// </summary>
	public void TakeDamage(int damage, BBClient attacker, string method)
	{
		if (!IsAlive || IsProxy) return;
		if (damage <= 0) return;

		_lastDamageAttacker = attacker;
		_lastDamageMethod = method;

		CurrentHealth -= damage;
		CurrentHealth = System.Math.Max(CurrentHealth, 0);

		DispatchDamagedEvent(damage, attacker, method);
		BroadcastHealthUpdate();
	}

	private void DispatchDamagedEvent(int damage, BBClient attacker, string method)
	{
		var victimClient = OwnerClient;
		if (!victimClient.IsValid())
			return;

		var victimSteamId = victimClient.SteamId;
		var victimName = victimClient.DisplayName;

		var attackerSteamId = attacker?.SteamId ?? 0;
		var attackerName = attacker?.DisplayName ?? "World";

		BroadcastPlayerDamagedEvent(
			victimSteamId,
			victimName,
			attackerSteamId,
			attackerName,
			damage,
			method
		);
	}

	private void DispatchKilledEvent()
	{
		if (_deathDispatched)
			return;

		_deathDispatched = true;

		var victimClient = OwnerClient;
		if (!victimClient.IsValid())
			return;

		var victimSteamId = victimClient.SteamId;
		var victimName = victimClient.DisplayName;

		var killerSteamId = _lastDamageAttacker?.SteamId ?? 0;
		var killerName = _lastDamageAttacker?.DisplayName ?? "World";

		BroadcastPlayerKilledEvent(
			victimSteamId,
			victimName,
			killerSteamId,
			killerName,
			_lastDamageMethod
		);
	}

	[Rpc.Broadcast]
	private void BroadcastPlayerDamagedEvent(
		ulong victimSteamId,
		string victimName,
		ulong attackerSteamId,
		string attackerName,
		int damage,
		string method
	)
	{
		GameEvents.Raise(new PlayerDamagedEvent(
			victimSteamId,
			victimName,
			attackerSteamId,
			attackerName,
			damage,
			method
		));
	}

	[Rpc.Broadcast]
	private void BroadcastPlayerKilledEvent(
		ulong victimSteamId,
		string victimName,
		ulong killerSteamId,
		string killerName,
		string method
	)
	{
		GameEvents.Raise(new PlayerKilledEvent(
			victimSteamId,
			victimName,
			killerSteamId,
			killerName,
			method
		));
	}

	/// <summary>
	/// Heals the player.
	/// </summary>
	public void Heal(int amount)
	{
		if (!IsAlive || IsProxy) return;

		CurrentHealth += amount;
		CurrentHealth = System.Math.Min(CurrentHealth, MaxHealth);
		BroadcastHealthUpdate();
	}

	/// <summary>
	/// Marks a contract as completed and applies skill/reward progression.
	/// </summary>
	public void CompleteContract(int rewardAmount)
	{
		if (IsProxy) return;

		ContractsCompleted++;
		SkillLevel += 0.1f;
		AddCurrency(rewardAmount);

		BroadcastContractCompletion();
	}

	/// <summary>
	/// Records a raid initiation.
	/// </summary>
	public void InitiateRaid()
	{
		if (IsProxy) return;

		IsRaidingNow = true;
		RaidsInitiated++;
		BroadcastRaidStatus();
	}

	/// <summary>
	/// Records raid defense participation.
	/// </summary>
	public void DefendRaid()
	{
		if (IsProxy) return;

		RaidsDefended++;
		BroadcastRaidStatus();
	}

	/// <summary>
	/// Broadcasts currency update to all clients.
	/// </summary>
	[Rpc.Broadcast]
	private void BroadcastCurrencyUpdate()
	{
		// Log.Info($"{PlayerName} now has {Currency}");
		// UI updates currency display
	}

	/// <summary>
	/// Broadcasts health update to all clients.
	/// </summary>
	[Rpc.Broadcast]
	private void BroadcastHealthUpdate()
	{
		Log.Info($"{PlayerName} health: {CurrentHealth}/{MaxHealth}");
		// UI updates health display
	}




	/// <summary>
	/// Broadcasts player death to all clients.
	/// </summary>
	[Rpc.Broadcast]
	private void BroadcastPlayerDeath()
	{
		Log.Warning($"{PlayerName} has died!");
		// Trigger death effects/respawn
	}

	/// <summary>
	/// Broadcasts contract completion to all clients.
	/// </summary>
	[Rpc.Broadcast]
	private void BroadcastContractCompletion()
	{
		Log.Info($"{PlayerName} completed a contract! (x{ContractsCompleted})");
	}

	/// <summary>
	/// Broadcasts raid status changes to all clients.
	/// </summary>
	[Rpc.Broadcast]
	private void BroadcastRaidStatus()
	{
		Log.Info($"{PlayerName} raid status - Initiated: {RaidsInitiated}, Defended: {RaidsDefended}");
	}
}
