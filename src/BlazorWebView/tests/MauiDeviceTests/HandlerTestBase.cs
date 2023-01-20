using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.DeviceTests;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public partial class HandlerTestBase : TestBase, IDisposable
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

			additionalCreationActions?.Invoke(appBuilder);

			_mauiApp = appBuilder.Build();

			_mauiContext = new ContextStub(_mauiApp.Services);
		}

		public void Dispose()
		{
			((IDisposable)_mauiApp)?.Dispose();

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
