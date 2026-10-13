using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Maui.HybridApp.UI;

namespace Maui.Controls.Sample.HybridWebApp;

public sealed class App : Application
{
	readonly IServiceScopeFactory _scopeFactory;
	readonly HybridWindowHost _windows;

	public App(IServiceScopeFactory scopeFactory, HybridWindowHost windows)
	{
		_scopeFactory = scopeFactory;
		_windows = windows;
		windows.Bind(this, request => CreatePlaygroundWindow(request.Location, request.Id));
	}

	protected override Window CreateWindow(IActivationState? activationState) => CreatePlaygroundWindow("index.html#/welcome");

	Window CreatePlaygroundWindow(string location, string? id = null)
	{
		_windows.ValidateLocation(location);
		var scope = _scopeFactory.CreateScope();
		try
		{
			var page = scope.ServiceProvider.GetRequiredService<MainPage>();
			var window = new OwnedWindow(page, scope)
			{
				Title = ".NET MAUI Playground — packaged"
			};
			if (OperatingSystem.IsWindows() || OperatingSystem.IsMacCatalyst())
			{
				window.Width = 1180;
				window.Height = 820;
			}
#if DEBUG
			if (MauiProgram.Development?.Upstream is not null)
				window.Title = ".NET MAUI Playground — Vite";
#endif
			var context = scope.ServiceProvider.GetRequiredService<HybridViewContext>();
			context.Bind(page, window, location, id);
			_windows.Track(context);
			return window;
		}
		catch
		{
			scope.Dispose();
			throw;
		}
	}

	sealed class OwnedWindow : Window, IDisposable
	{
		readonly MainPage _page;
		readonly IServiceScope _scope;
		bool _disposed;

		public OwnedWindow(MainPage page, IServiceScope scope) : base(new NavigationPage(page))
		{
			NavigationPage.SetHasNavigationBar(page, false);
			_page = page;
			_scope = scope;
			Destroying += Destroyed;
		}
		void Destroyed(object? sender, EventArgs args) => Dispose();
		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;
			Destroying -= Destroyed;
			try { _page.Stop(); }
			finally { _scope.Dispose(); }
		}
	}
}
