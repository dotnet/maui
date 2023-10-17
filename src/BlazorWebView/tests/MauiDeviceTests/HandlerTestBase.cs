using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public partial class HandlerTestBase : TestBase, IAsyncDisposable
	{
		private MauiApp _mauiApp;
		private IMauiContext _mauiContext;
		private bool _isCreated;

		public void EnsureHandlerCreated(Action<MauiAppBuilder> additionalCreationActions = null)
		{
			if (_isCreated)
			{
				return;
			}

			_isCreated = true;
			var appBuilder = MauiApp
				.CreateBuilder();

			appBuilder.Services.AddSingleton<IDispatcherProvider>(svc => TestDispatcher.Provider);
			appBuilder.Services.AddScoped<IDispatcher>(svc => TestDispatcher.Current);
			appBuilder.Services.AddSingleton<IApplication>((_) => new CoreApplicationStub());

			additionalCreationActions?.Invoke(appBuilder);

			_mauiApp = appBuilder.Build();

			_mauiContext = new ContextStub(_mauiApp.Services);
		}

		public async ValueTask DisposeAsync()
		{
			if (_mauiApp != null)
			{
				await ((IAsyncDisposable)_mauiApp).DisposeAsync();
			}

			_mauiApp = null;
			_mauiContext = null;
		}

		protected IMauiContext MauiContext
		{
			get
			{
				EnsureHandlerCreated();
				return _mauiContext;
			}
		}

		protected THandler CreateHandler<THandler>(IView view)
			where THandler : IViewHandler
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			var size = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
			view.Arrange(new Rect(0, 0, size.Width, size.Height));
			handler.PlatformArrange(view.Frame);

			return handler;
		}

		protected async Task<THandler> CreateHandlerAsync<THandler>(IView view) where THandler : IViewHandler =>
			await InvokeOnMainThreadAsync(() => CreateHandler<THandler>(view));

		protected Task<TValue> GetValueAsync<TValue, THandler>(IView view, Func<THandler, TValue> func)
			 where THandler : IViewHandler
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				return func(handler);
			});
		}
	}
}
