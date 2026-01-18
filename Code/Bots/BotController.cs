using Sandbox;
using Basebound.Player;

namespace Basebound.Bots;

/// <summary>
/// Minimal stub for bot control behavior.
/// </summary>
public sealed class BotController : Component
{
	[RequireComponent]
	public PlayerClient Client { get; private set; }

	[Property, Title("Bot Name")]
	public string BotName { get; set; } = "Bot";

	protected override void OnStart()
	{
		base.OnStart();

		if (IsProxy || !Client.IsValid())
			return;

		if (string.IsNullOrWhiteSpace(BotName))
			BotName = Client.DisplayName;

		Log.Info($"BotController started for {BotName}.");
	}
}
