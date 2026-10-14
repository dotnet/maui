namespace HybridApp.ExternalProvider;

// Referenced annotations alone are inert; the packaged consuming assembly explicitly roots this interface.
public interface INativeCounterContract
{
	int Value { [HybridAction("plugin.contract.value")] get; }
	[HybridAction("plugin.contract.add")]
	int Add(int increment);
}
