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
#if IOS
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
					handlers.AddHandler(typeof(Controls.Window), typeof(WindowHandlerStub));
					handlers.AddHandler(typeof(Controls.ContentPage), typeof(PageHandler));
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
#if ANDROID
				// If the Android views don't have LayoutParams set, updating some properties (e.g., Text)
				// can run into issues when deciding whether a re-layout is required. Normally this isn't
				// an issue because the LayoutParams get set when the view is added to a ViewGroup, but 
				// since we're not doing that here, we need to ensure they have LayoutParams so that tests
				// which update properties don't crash. 

				var aView = viewHandler.PlatformView as Android.Views.View;
				if (aView.LayoutParameters == null)
				{
					aView.LayoutParameters =
						new Android.Views.ViewGroup.LayoutParams(
							Android.Views.ViewGroup.LayoutParams.WrapContent,
							Android.Views.ViewGroup.LayoutParams.WrapContent);
				}
#endif

				view.Arrange(new Rect(0, 0, view.Width, view.Height));
				viewHandler.PlatformArrange(view.Frame);
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
		protected Task CreateHandlerAndAddToWindow<THandler>(IElement view, Action<THandler> action)
			where THandler : class, IElementHandler
		{
			return CreateHandlerAndAddToWindow<THandler>(view, handler =>
			{
				action(handler);
				return Task.CompletedTask;
			});
		}

		protected Task CreateHandlerAndAddToWindow<THandler>(IElement view, Func<THandler, Task> action)
			where THandler : class, IElementHandler
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				IWindow window = null;

				if (view is IWindow w)
				{
					window = w;
				}
				else if (view is Page page)
				{
					window = new Controls.Window(page);
				}
				else
				{
					window = new Controls.Window(new ContentPage() { Content = (View)view });
				}

				await RunWindowTest<THandler>(window, (handler) => action(handler as THandler));
			});
		}

		protected void OnLoaded(VisualElement frameworkElement, Action action)
		{
			if (frameworkElement.IsLoaded)
			{
				action();
				return;
			}

			EventHandler loaded = null;

			loaded = (_, __) =>
			{
				if (loaded != null)
					frameworkElement.Loaded -= loaded;

				action();
			};

			frameworkElement.Loaded += loaded;
		}


		protected void OnUnloaded(VisualElement frameworkElement, Action action)
		{
			if (!frameworkElement.IsLoaded)
			{
				action();
				return;
			}

			EventHandler unloaded = null;

			unloaded = (_, __) =>
			{
				if (unloaded != null)
					frameworkElement.Unloaded -= unloaded;

				action();
			};

			frameworkElement.Unloaded += unloaded;
		}

		protected Task OnUnloadedAsync(VisualElement frameworkElement, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			OnUnloaded(frameworkElement, () => taskCompletionSource.SetResult(true));
			return taskCompletionSource.Task.WaitAsync(timeOut.Value);
		}

		protected Task OnLoadedAsync(VisualElement frameworkElement, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			OnLoaded(frameworkElement, () => taskCompletionSource.SetResult(true));
			return taskCompletionSource.Task.WaitAsync(timeOut.Value);
		}
	}
}