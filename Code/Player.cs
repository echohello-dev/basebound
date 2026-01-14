using Sandbox;

/// <summary>
/// Core player component managing identity, progression, and state.
/// Handles currency, health, skill tracking, and game system integration.
/// </summary>
public sealed class Player : Component
{
	// Player Identity
	[Property] public string PlayerName { get; set; } = "Player";
	[Property] public int PlayerLevel { get; set; } = 1;

	// Economy System
	[Property] public long Currency { get; set; } = 1000;
	[Property] public long CurrencyPerTick { get; set; } = 5;
	[Property] public bool IsTaxed { get; set; } = false;

	// Health & Status
	[Property] public int MaxHealth { get; set; } = 100;
	[Property] public int CurrentHealth { get; set; } = 100;
	[Property] public bool IsAlive { get; set; } = true;

	// Progression & Skills
	[Property] public int ContractsCompleted { get; set; } = 0;
	[Property] public float SkillLevel { get; set; } = 1f;
	[Property] public long TotalEarnings { get; set; } = 0;

	// Raid System
	[Property] public bool IsRaidingNow { get; set; } = false;
	[Property] public int RaidsInitiated { get; set; } = 0;
	[Property] public int RaidsDefended { get; set; } = 0;

	// Base Building
	[Property] public bool HasBase { get; set; } = false;
	[Property] public int BlocksPlaced { get; set; } = 0;

	// State tracking
	private float _currencyTickAccumulator = 0f;

	protected override void OnStart()
	{
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
		if (!IsAlive || IsProxy) return;

		CurrentHealth -= damage;
		CurrentHealth = System.Math.Max(CurrentHealth, 0);
		BroadcastHealthUpdate();
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
		Log.Info($"{PlayerName} now has {Currency}");
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
