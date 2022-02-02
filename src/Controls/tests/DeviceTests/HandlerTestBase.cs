using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase : TestBase, IDisposable
	{
		bool _isCreated;
		MauiApp _mauiApp;
		IMauiContext _mauiContext;

		public void EnsureHandlerCreated(Action<MauiAppBuilder> additionalCreationActions = null)
		{
			if (_isCreated)
			{
				return;
			}

			_isCreated = true;
			var appBuilder = MauiApp
				.CreateBuilder()
				.RemapForControls()
				.ConfigureLifecycleEvents(lifecycle =>
				{
#if __IOS__
					lifecycle
						.AddiOS(iOS => iOS
							.OpenUrl((app, url, options) =>
								Microsoft.Maui.Essentials.Platform.OpenUrl(app, url, options))
							.ContinueUserActivity((application, userActivity, completionHandler) =>
								Microsoft.Maui.Essentials.Platform.ContinueUserActivity(application, userActivity, completionHandler))
							.PerformActionForShortcutItem((application, shortcutItem, completionHandler) =>
								Microsoft.Maui.Essentials.Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler)));
#elif WINDOWS
					lifecycle
						.AddWindows(windows =>
						{
							windows
								.OnLaunched((app, e) =>
									Microsoft.Maui.Essentials.Platform.OnLaunched(e));
							windows
								.OnActivated((window, e) =>
									Microsoft.Maui.Essentials.Platform.OnActivated(window, e));
						});
#endif
				})
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
#if WINDOWS
					handlers.AddHandler(typeof(Controls.Window), typeof(WindowHandlerStub));
#endif
				});

			additionalCreationActions?.Invoke(appBuilder);

			_mauiApp = appBuilder.Build();

			_mauiContext = new ContextStub(_mauiApp.Services);
		}

		public void Dispose()
		{
			((IDisposable)_mauiApp).Dispose();

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

		protected THandler CreateHandler<THandler>(IElement view)
			where THandler : IElementHandler
		{
			return CreateHandler<THandler>(view, MauiContext);
		}

		protected THandler CreateHandler<THandler>(IElement element, IMauiContext mauiContext)
			where THandler : IElementHandler
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetMauiContext(mauiContext);

			handler.SetVirtualView(element);
			element.Handler = handler;

			if (element is IView view && handler is IViewHandler viewHandler)
			{
				view.Arrange(new Rectangle(0, 0, view.Width, view.Height));
				viewHandler.NativeArrange(view.Frame);
			}

			return handler;
		}

		protected async Task<THandler> CreateHandlerAsync<THandler>(IElement view) where THandler : IElementHandler =>
			await InvokeOnMainThreadAsync(() => CreateHandler<THandler>(view));

		protected Task<TValue> GetValueAsync<TValue, THandler>(IElement view, Func<THandler, TValue> func)
			 where THandler : IElementHandler
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				return func(handler);
			});
		}
	}
}