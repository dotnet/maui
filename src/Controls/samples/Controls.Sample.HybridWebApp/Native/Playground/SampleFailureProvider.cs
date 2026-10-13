using Maui.HybridApp;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleFailureProvider : IHybridCommandProvider
{
	public void Register(CommandRegistry registry) =>
		registry.Register<EmptyArgs, Ack>("sample.fail", ProtocolJsonContext.Default.EmptyArgs, ProtocolJsonContext.Default.Ack,
			(_, _) => throw new CommandException("intentional_error", "This is the deliberate native failure. The bridge is still usable."));
}
