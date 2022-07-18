using System;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Devices;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase : TestBase, IDisposable
	{
		bool _isCreated;
		protected MauiApp MauiApp { get; private set; }
		IMauiContext _mauiContext;

		// In order to run any page level tests android needs to add itself to the decor view inside a new fragment
		// that way all the lifecycle events related to being attached to the window will fire
		// adding/removing that many fragments in parallel to the decor view was causing the tests to be unreliable
		// That being said...
		// There's definitely a chance that the code written to manage this process could be improved		
		public const string RunInNewWindowCollection = "Serialize test because it has to add itself to the main window";

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
								ApplicationModel.Platform.OpenUrl(app, url, options))
							.ContinueUserActivity((application, userActivity, completionHandler) =>
								ApplicationModel.Platform.ContinueUserActivity(application, userActivity, completionHandler))
							.PerformActionForShortcutItem((application, shortcutItem, completionHandler) =>
								ApplicationModel.Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler)));
#elif WINDOWS
					lifecycle
						.AddWindows(windows =>
						{
							windows
								.OnLaunched((app, e) =>
									ApplicationModel.Platform.OnLaunched(e));
							windows
								.OnActivated((window, e) =>
									ApplicationModel.Platform.OnActivated(window, e));
						});
#endif
				})
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Controls.Window), typeof(WindowHandlerStub));
					handlers.AddHandler(typeof(Controls.ContentPage), typeof(PageHandler));
					handlers.AddHandler(typeof(MauiAppNewWindowStub), typeof(ApplicationHandler));
				});

			appBuilder.Services.AddSingleton<IDispatcherProvider>(svc => TestDispatcher.Provider);
			appBuilder.Services.AddScoped<IDispatcher>(svc => TestDispatcher.Current);
			appBuilder.Services.TryAddSingleton<IApplication>((_) => new Application());

			additionalCreationActions?.Invoke(appBuilder);

			MauiApp = appBuilder.Build();

			_mauiContext = new ContextStub(MauiApp.Services);
		}

		public void Dispose()
		{
			((IDisposable)MauiApp)?.Dispose();

			MauiApp = null;
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

				var size = view.Measure(view.Width, view.Height);
				var w = size.Width;
				var h = size.Height;
#elif IOS
				var size = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
				var w = size.Width;
				var h = size.Height;
#else
				// Windows cannot measure without the view being loaded
				// iOS needs more love when I get an IDE again
				var w = view.Width.Equals(double.NaN) ? -1 : view.Width;
				var h = view.Height.Equals(double.NaN) ? -1 : view.Height;
#endif

				view.Arrange(new Rect(0, 0, w, h));
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

		protected Task SetValueAsync<TValue, THandler>(IView view, TValue value, Action<THandler, TValue> func)
			where THandler : IElementHandler
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				func(handler, value);
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

		static SemaphoreSlim _takeOverMainContentSempahore = new SemaphoreSlim(1);
		protected Task CreateHandlerAndAddToWindow<THandler>(IElement view, Func<THandler, Task> action, IMauiContext mauiContext = null)
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

				try
				{
					await _takeOverMainContentSempahore.WaitAsync();

					await SetupWindowForTests<THandler>(window, async () =>
					{
						IView content = window.Content;

						if (content is IPageContainer<Page> pc)
						{
							content = pc.CurrentPage;
							if (content == null)
							{
								// This is mainly a timing issue with Shell.
								// Basically the `CurrentPage` on Shell isn't initialized until it's
								// actually navigated to because it's a DataTemplate.
								// The CurrentPage doesn't come into existence until the platform requests it.
								// The initial `Navigated` events on Shell all fire a bit too early as well.
								// Ideally I'd just use that instead of having to add a delay.
								await Task.Delay(100);
								content = pc.CurrentPage;
							}

							_ = content ?? throw new InvalidOperationException("Current Page Not Initialized");
						}

						await OnLoadedAsync(content as VisualElement);
#if WINDOWS
						await Task.Delay(10);
#endif
						if (typeof(THandler).IsAssignableFrom(window.Handler.GetType()))
							await action((THandler)window.Handler);
						else if (typeof(THandler).IsAssignableFrom(window.Content.Handler.GetType()))
							await action((THandler)window.Content.Handler);
						else if (window.Content is ContentPage cp && typeof(THandler).IsAssignableFrom(cp.Content.Handler.GetType()))
							await action((THandler)cp.Content.Handler);
						else
							throw new Exception($"I can't work with {typeof(THandler)}");
					}, mauiContext);
				}
				finally
				{
					_takeOverMainContentSempahore.Release();
				}
			});
		}

		async protected Task ValidatePropertyInitValue<TValue, THandler>(
			IView view,
			Func<TValue> GetValue,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedValue)
			where THandler : IElementHandler
		{
			var values = await GetValueAsync(view, (THandler handler) =>
			{
				return new
				{
					ViewValue = GetValue(),
					PlatformViewValue = GetPlatformValue(handler)
				};
			});

			Assert.Equal(expectedValue, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		async protected Task ValidatePropertyUpdatesValue<TValue, THandler>(
			IView view,
			string property,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedSetValue,
			TValue expectedUnsetValue)
			where THandler : IElementHandler
		{
			var propInfo = view.GetType().GetProperty(property);

			// set initial values

			propInfo.SetValue(view, expectedSetValue);

			var (handler, viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				return (handler, (TValue)propInfo.GetValue(view), GetPlatformValue(handler));
			});

			Assert.Equal(expectedSetValue, viewVal);
			Assert.Equal(expectedSetValue, nativeVal);

			await ValidatePropertyUpdatesAfterInitValue(handler, property, GetPlatformValue, expectedSetValue, expectedUnsetValue);
		}

		async protected Task ValidatePropertyUpdatesAfterInitValue<TValue, THandler>(
			THandler handler,
			string property,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedSetValue,
			TValue expectedUnsetValue)
			where THandler : IElementHandler
		{
			var view = handler.VirtualView;
			var propInfo = handler.VirtualView.GetType().GetProperty(property);

			// confirm can update

			var (viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				propInfo.SetValue(view, expectedUnsetValue);
				handler.UpdateValue(property);

				return ((TValue)propInfo.GetValue(view), GetPlatformValue(handler));
			});

			Assert.Equal(expectedUnsetValue, viewVal);
			Assert.Equal(expectedUnsetValue, nativeVal);

			// confirm can revert

			(viewVal, nativeVal) = await InvokeOnMainThreadAsync(() =>
			{
				propInfo.SetValue(view, expectedSetValue);
				handler.UpdateValue(property);

				return ((TValue)propInfo.GetValue(view), GetPlatformValue(handler));
			});

			Assert.Equal(expectedSetValue, viewVal);
			Assert.Equal(expectedSetValue, nativeVal);
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

		protected async Task OnNavigatedToAsync(Page page, TimeSpan? timeOut = null)
		{
			await OnLoadedAsync(page, timeOut);

			if (page.HasNavigatedTo)
				return;

			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

			page.NavigatedTo += NavigatedTo;

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);
			void NavigatedTo(object sender, NavigatedToEventArgs e)
			{
				taskCompletionSource.SetResult(true);
				page.NavigatedTo -= NavigatedTo;
			}
		}

		protected async Task OnFrameSetToNotEmpty(VisualElement frameworkElement, TimeSpan? timeOut = null)
		{
			if (frameworkElement.Frame.Height > 0 &&
				frameworkElement.Frame.Width > 0)
			{
				return;
			}

			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			frameworkElement.BatchCommitted += OnBatchCommitted;

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);

			// Wait for the layout to propagate to the platform
			await AssertionExtensions.Wait(
				() => !frameworkElement.GetBoundingBox().Size.Equals(Size.Zero)
			);

			void OnBatchCommitted(object sender, Controls.Internals.EventArg<VisualElement> e)
			{
				if (frameworkElement.Frame.Height <= 0 ||
					frameworkElement.Frame.Width <= 0)
				{
					return;
				}

				frameworkElement.BatchCommitted -= OnBatchCommitted;
				taskCompletionSource.SetResult(true);
			}
		}
	}
}
