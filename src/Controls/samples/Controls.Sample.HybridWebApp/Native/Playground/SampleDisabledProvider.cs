using Maui.HybridApp;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleDisabledProvider : IHybridCommandProvider
{
	public void Register(CommandRegistry registry) =>
		registry.Register("sample.disabled", ProtocolJsonContext.Default.EmptyArgs, ProtocolJsonContext.Default.Ack,
			(_, _) => Task.FromResult(new Ack(true)), enabled: false);
}
