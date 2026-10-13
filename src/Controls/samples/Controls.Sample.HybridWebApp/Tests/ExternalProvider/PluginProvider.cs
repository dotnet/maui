using System.Text.Json;
using Maui.HybridApp;

namespace HybridApp.ExternalProvider;

public sealed class PluginProvider : IHybridCommandProvider
{
	public void Register(CommandRegistry registry)
	{
		var json = PluginJsonContext.Default;
		registry.RegisterTopic("plugin.changed");
		registry.Register("plugin.echo", json.PluginArgs, json.PluginResult, (args, op) =>
		{
			var result = new PluginResult(args.Text);
			op.Publish("plugin.changed", JsonSerializer.SerializeToElement(result, json.PluginResult));
			return Task.FromResult(result);
		});
		registry.Register("plugin.disabled", json.PluginArgs, json.PluginResult,
			(args, _) => Task.FromResult(new PluginResult(args.Text)), enabled: false);
	}
}
