namespace Maui.HybridApp;

/// <summary>An optional minimal contract for explicitly composed external providers.</summary>
public interface IHybridCommandProvider
{
	/// <summary>Registers this provider's commands, metadata and topics in the host registry.</summary>
	/// <param name="registry">The host's registry.</param>
	void Register(CommandRegistry registry);
}
