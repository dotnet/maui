namespace HybridApp.ExternalProvider;

public record PluginEventValue(int Value);

public interface IPluginEventContract
{
	[HybridEvent("plugin.native.changed")]
	event EventHandler<PluginEventValue> Changed;
}

public sealed class PluginEventSource : IPluginEventContract
{
	public event EventHandler<PluginEventValue>? Changed;
	public void Raise(int value) => Changed?.Invoke(this, new(value));
}
