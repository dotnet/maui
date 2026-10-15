using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.HybridWebApps;
using Microsoft.Maui.Controls.HybridWebApps.UI;

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
		HybridServiceProvider? local = null;
		MainPage? page = null;
		try
		{
			local = scope.ServiceProvider.CreateHybridAppProvider();
			page = new MainPage(local.GetRequiredService<HybridAppTargetObject>(), local.GetRequiredService<HybridViewContext>());
			var window = new OwnedWindow(page, local, scope)
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
			var context = local.GetRequiredService<HybridViewContext>();
			context.Bind(page, window, location, id);
			_windows.Track(context);
			return window;
		}
		catch
		{
			ObserveCleanup(CleanupAsync(page, local, scope));
			throw;
		}
	}

	static async Task CleanupAsync(MainPage? page, HybridServiceProvider? local, IServiceScope scope)
	{
		try
		{ page?.Stop(); }
		finally
		{
			try
			{ if (local is not null) await local.DisposeAsync(); }
			finally
			{
				if (scope is IAsyncDisposable asynchronous)
					await asynchronous.DisposeAsync();
				else
					scope.Dispose();
			}
		}
	}
	static void ObserveCleanup(Task cleanup)
	{
		if (!cleanup.IsCompletedSuccessfully)
			_ = ReportCleanupAsync(cleanup);
	}
	static async Task ReportCleanupAsync(Task cleanup)
	{
		try
		{ await cleanup; }
		catch (Exception error) { Console.WriteLine($"HYBRIDWEBAPP_CLEANUP_ERROR type={error.GetType().Name}"); }
	}

	sealed class OwnedWindow : Window, IDisposable, IAsyncDisposable
	{
		readonly MainPage _page;
		readonly IServiceScope _scope;
		readonly HybridServiceProvider _local;
		readonly object _cleanupGate = new();
		Task? _cleanup;

		public OwnedWindow(MainPage page, HybridServiceProvider local, IServiceScope scope) : base(new NavigationPage(page))
		{
			NavigationPage.SetHasNavigationBar(page, false);
			_page = page;
			_scope = scope;
			_local = local;
			Destroying += Destroyed;
		}
		void Destroyed(object? sender, EventArgs args) => Dispose();
		public void Dispose() => ObserveCleanup(DisposeAsync().AsTask());
		public ValueTask DisposeAsync()
		{
			TaskCompletionSource completion;
			lock (_cleanupGate)
			{
				if (_cleanup is not null)
					return new(_cleanup);
				Destroying -= Destroyed;
				completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
				_cleanup = completion.Task;
			}
			_ = CompleteCleanupAsync(completion);
			return new(completion.Task);
		}
		async Task CompleteCleanupAsync(TaskCompletionSource completion)
		{
			try
			{
				await CleanupAsync(_page, _local, _scope);
				completion.SetResult();
			}
			catch (Exception error) { completion.SetException(error); }
		}
	}
}
