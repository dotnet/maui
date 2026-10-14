using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.HybridApp;

/// <summary>Host policy for deliberately imported SDK actions; annotations are not permission grants.</summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class HybridNativePolicy
{
	/// <summary>Validates imported native inputs before making a direct SDK call.</summary>
	/// <param name="services">The owning proxy.</param><param name="name">The declared action.</param>
	/// <param name="arguments">The validated argument envelope.</param>
	public static void Validate(IServiceProvider services, string name, JsonElement arguments)
	{
		if (name == "clipboard.write")
			ValidateText(services, Value("text", 0));
		if (name == "geolocation.location")
		{
			var request = arguments.ValueKind == JsonValueKind.Object ? arguments.GetProperty("request") : arguments[0];
			if (!request.TryGetProperty("timeout", out var timeout) || timeout.ValueKind != JsonValueKind.String ||
				!TimeSpan.TryParse(timeout.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var duration) ||
				duration <= TimeSpan.Zero || duration > TimeSpan.FromMinutes(2)) throw Invalid();
		}
		if (name.StartsWith("dialogs.", StringComparison.Ordinal))
		{
			var confirm = name == "dialogs.confirm";
			if (name is "dialogs.alert" or "dialogs.confirm")
			{
				Label(Value("cancel", confirm ? 3 : 2));
				if (confirm) Label(Value("accept", 2));
			}
			if (name == "dialogs.actionSheet")
			{
				var cancel = Value("cancel", 1); if (cancel is not null) Label(cancel);
				var destruction = Value("destruction", 2); if (destruction is not null) Label(destruction);
				var buttons = arguments.ValueKind == JsonValueKind.Object ? arguments.GetProperty("buttons") : arguments[4];
				if (buttons.GetArrayLength() > 64) throw Invalid();
				foreach (var button in buttons.EnumerateArray()) Label(button.GetString());
			}
		}
		string? Value(string key, int index) => arguments.ValueKind == JsonValueKind.Object ?
			arguments.GetProperty(key).GetString() : arguments[index].GetString();
		static void Label(string? text) { if (string.IsNullOrWhiteSpace(text)) throw Invalid(); }
	}

	/// <summary>Applies the same clipboard text budget to native reads and writes.</summary>
	/// <param name="services">The owning proxy.</param><param name="text">Native text, if any.</param>
	public static void ValidateText(IServiceProvider services, string? text)
	{
		if (text?.Length > services.GetRequiredService<HybridAppOptions>().MaximumTextLength) throw Invalid();
	}
	static CommandException Invalid() => new("invalid_arguments", "Imported native inputs exceed the host policy or contain blank labels.");
}
