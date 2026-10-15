using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.HybridWebApps;

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
				duration <= TimeSpan.Zero || duration > TimeSpan.FromMinutes(2))
				throw Invalid();
		}
		string? Value(string key, int index) => arguments.ValueKind == JsonValueKind.Object ?
			arguments.GetProperty(key).GetString() : arguments[index].GetString();
	}

	/// <summary>Applies the same clipboard text budget to native reads and writes.</summary>
	/// <param name="services">The owning proxy.</param><param name="text">Native text, if any.</param>
	public static void ValidateText(IServiceProvider services, string? text)
	{
		if (text?.Length > services.GetRequiredService<HybridAppOptions>().MaximumTextLength)
			throw Invalid();
	}
	static CommandException Invalid() => new("invalid_arguments", "Imported native inputs exceed the host policy.");
}
