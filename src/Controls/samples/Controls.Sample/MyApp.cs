using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;

namespace Maui.Controls.Sample
{
	public class MyApp : MauiApp
	{
		// IAppState state
		public override IWindow CreateWindow(IActivationState state)
		{
			Forms.Init(state);
			return Services.GetRequiredService<IWindow>();
		}
	}
}