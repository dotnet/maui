using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample.HybridWebApp;

public sealed class App(IServiceScopeFactory scopeFactory) : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var scope = scopeFactory.CreateScope();
		try
		{
			var page = scope.ServiceProvider.GetRequiredService<MainPage>();
			var window = new Window(page)
			{
				Title = ".NET MAUI Playground — packaged",
				Width = 1180,
				Height = 820
			};
#if DEBUG
			if (MauiProgram.Development?.Upstream is not null)
				window.Title = ".NET MAUI Playground — Vite";
#endif
			window.Destroying += Destroying;
			return window;

			void Destroying(object? sender, EventArgs args)
			{
				window.Destroying -= Destroying;
				try
				{
					page.Stop();
				}
				finally
				{
					scope.Dispose();
				}
			}
		}
		catch
		{
			scope.Dispose();
			throw;
		}
	}
}
