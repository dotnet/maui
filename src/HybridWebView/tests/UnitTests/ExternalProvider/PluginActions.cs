using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.HybridWebApps;

namespace HybridApp.ExternalProvider;

// These action DTOs deliberately have no user-declared JsonSerializerContext or JsonSerializable list.
public sealed record PluginActionValue(string Text, PluginActionKind Kind, string? Note = null, List<int>? Values = null);
public enum PluginActionKind { First, Second }
public sealed class PluginActionState { public int Calls { get; set; } }

[HybridTopic("plugin.changed")]
public sealed class PluginActions(PluginActionState state)
{
	[HybridAction("plugin.echo")]
	public PluginResult Echo(string text, HybridActionContext context)
	{
		var result = new PluginResult(text);
		context.Publish("plugin.changed", result, PluginJsonContext.Default.PluginResult);
		return result;
	}

	[HybridAction("plugin.disabled", Enabled = false)]
	public PluginResult Disabled(string text) => new(text);

	[HybridAction("plugin.action.echo")]
	public PluginActionValue Echo(PluginActionValue value) => value;

	[HybridAction("plugin.action.count")]
	public int Count(int increment = 1) => state.Calls += increment;

	[HybridAction("plugin.action.services")]
	public int Services([FromServices] PluginActionState current,
		[FromKeyedServices("action-key")] PluginActionState keyed) => current.Calls + keyed.Calls;

	[HybridAction("plugin.action.static")]
	public static string Static(string value) => value;
}

public static class PluginStaticActions
{
	[HybridAction("plugin.action.staticClass")]
	public static int Double(int value) => value * 2;
}

public sealed class PluginNormalizedText
{
	public string Text { get; }
	public PluginNormalizedText(string? text) => Text = text ?? "";
}
public sealed record PluginDefaultMode(PluginActionKind? Kind = PluginActionKind.Second);

public sealed class PluginConstructorActions
{
	[HybridAction("plugin.action.normalized")]
	public static PluginNormalizedText Normalize(PluginNormalizedText value) => value;

	[HybridAction("plugin.action.enumDefault")]
	public static PluginActionKind? Default(PluginActionKind? kind = PluginActionKind.Second) => kind;

	[HybridAction("plugin.action.dtoDefault")]
	public static PluginDefaultMode DtoDefault(PluginDefaultMode value) => value;
}
