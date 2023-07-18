using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase : HandlerTestBase, IDisposable
	{
		// In order to run any page level tests android needs to add itself to the decor view inside a new fragment
		// that way all the lifecycle events related to being attached to the window will fire
		// adding/removing that many fragments in parallel to the decor view was causing the tests to be unreliable
		// That being said...
		// There's definitely a chance that the code written to manage this process could be improved		
		public const string RunInNewWindowCollection = "Serialize test because it has to add itself to the main window";

		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
		{
			mauiAppBuilder.Services.AddSingleton<IApplication>((_) => new ApplicationStub());
			return mauiAppBuilder.ConfigureTestBuilder();
		}

		protected void SetupShellHandlers(IMauiHandlersCollection handlers) =>
			handlers.SetupShellHandlers();

		protected THandler CreateHandler<THandler>(IElement view)
			where THandler : IElementHandler, new()
		{
			return CreateHandler<THandler>(view, MauiContext);
		}

		protected async Task<THandler> CreateHandlerAsync<THandler>(IElement view)
			where THandler : IElementHandler, new() =>
			await InvokeOnMainThreadAsync(() => CreateHandler<THandler>(view));

		protected IElementHandler CreateHandler(IElement view)
		{
			var handler = view.ToHandler(MauiContext);
			InitializeViewHandler(view, handler, MauiContext);
			return handler;
		}

		protected async Task<IElementHandler> CreateHandlerAsync(IElement view) =>
			await InvokeOnMainThreadAsync(() => CreateHandler(view));

		protected Task<TValue> GetValueAsync<TValue, THandler>(IElement view, Func<THandler, TValue> func)
			 where THandler : IElementHandler, new()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				return func(handler);
			});
		}

		protected Task<TValue> GetValueAsync<TValue>(IElement view, Func<IPlatformViewHandler, TValue> func)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = (IPlatformViewHandler)view.ToHandler(MauiContext);
				return func(handler);
			});
		}
		protected Task<TValue> GetValueAsync<TValue>(IElement view, Func<IPlatformViewHandler, Task<TValue>> func)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var handler = (IPlatformViewHandler)view.ToHandler(MauiContext);
				return await func(handler);
			});
		}

		IWindow CreateWindowForContent(IElement view)
		{
			IWindow window;

			if (view is IWindow w)
				window = w;
			else if (view is Page page)
				window = new Controls.Window(page);
			else
				window = new Controls.Window(new ContentPage() { Content = (View)view });

			return window;
		}

		protected Task CreateHandlerAndAddToWindow(IElement view, Action action)
		{
			return CreateHandlerAndAddToWindow<IWindowHandler>(CreateWindowForContent(view), handler =>
			{
				action();
				return Task.CompletedTask;
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
		protected Task CreateHandlerAndAddToWindow<THandler>(IElement view, Func<THandler, Task> action, IMauiContext mauiContext = null, TimeSpan? timeOut = null)
			where THandler : class, IElementHandler
		{
			mauiContext ??= MauiContext;

			timeOut ??= TimeSpan.FromSeconds(15);

			return InvokeOnMainThreadAsync(async () =>
			{
				IWindow window = CreateWindowForContent(view);

				var application = mauiContext.Services.GetService<IApplication>();

				if (application is ApplicationStub appStub)
				{
					appStub.SetWindow((Window)window);

					// Trigger the work flow of creating a window
					_ = application.CreateWindow(null);
				}

				try
				{
					await _takeOverMainContentSempahore.WaitAsync();

					await SetupWindowForTests<THandler>(window, async () =>
					{
						IView content = window.Content;

						if (content is FlyoutPage fp)
							content = fp.Detail;

						if (window is Window w && w.Navigation.ModalStack.Count > 0)
							content = w.Navigation.ModalStack.Last();

						if (content is IPageContainer<Page> pc)
						{
							content = pc.CurrentPage;
							if (content is null)
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

#if !WINDOWS
						if (window is Window controlsWindow)
						{
							if (!controlsWindow.IsActivated)
								window.Activated();
						}
						else
						{
							controlsWindow = null;
							window.Activated();
						}
#endif

#if WINDOWS
						await Task.Delay(10);
#endif

						THandler handler;

						if (typeof(THandler).IsAssignableFrom(window.Handler.GetType()))
							handler = (THandler)window.Handler;
						else if (typeof(THandler).IsAssignableFrom(window.Content.Handler.GetType()))
							handler = (THandler)window.Content.Handler;
						else if (window.Content is ContentPage cp && typeof(THandler).IsAssignableFrom(cp.Content.Handler.GetType()))
							handler = (THandler)cp.Content.Handler;
						else if (typeof(THandler).IsAssignableFrom(typeof(WindowHandler)))
							throw new Exception($"Use IWindowHandler instead of WindowHandler for CreateHandlerAndAddToWindow");
						else
							throw new Exception($"I can't work with {typeof(THandler)}");

						await action(handler).WaitAsync(timeOut.Value);


#if !WINDOWS
						bool isActivated = controlsWindow?.IsActivated ?? false;
						bool isDestroyed = controlsWindow?.IsDestroyed ?? false;

						if (isActivated)
							window.Deactivated();

						if (!isDestroyed)
							window.Destroying();
#endif

					}, mauiContext);
				}
				finally
				{
					_takeOverMainContentSempahore.Release();
				}
			});
		}

		/// <summary>
		/// This is more complicated as we have different logic depending on the view being focused or not.
		/// When we attach to the UI, there is only a single control so sometimes it cannot unfocus.
		/// </summary>
		public async Task AttachAndRunFocusAffectedControl<TType, THandler>(TType control, Action<THandler> action)
			where TType : IView, new()
			where THandler : class, IPlatformViewHandler, IElementHandler, new()
		{
			Func<THandler, Task> boop = (handler) =>
			{
				action.Invoke(handler);
				return Task.CompletedTask;
			};

			await AttachAndRunFocusAffectedControl<TType, THandler>(control, boop);
		}

		/// <summary>
		/// This is more complicated as we have different logic depending on the view being focused or not.
		/// When we attach to the UI, there is only a single control so sometimes it cannot unfocus.
		/// </summary>
		public async Task AttachAndRunFocusAffectedControl<TType, THandler>(TType control, Func<THandler, Task> action)
			where TType : IView, new()
			where THandler : class, IPlatformViewHandler, IElementHandler, new()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<VerticalStackLayout, LayoutHandler>();
					handler.AddHandler<TType, THandler>();
				});
			});

			var layout = new VerticalStackLayout
			{
				WidthRequest = 200,
				HeightRequest = 200,
			};

			var placeholder = new TType();
			layout.Add(placeholder);
			layout.Add(control);

			await AttachAndRun(layout, handler => action(control.Handler as THandler));
		}

		async protected Task ValidatePropertyInitValue<TValue, THandler>(
			IView view,
			Func<TValue> GetValue,
			Func<THandler, TValue> GetPlatformValue,
			TValue expectedValue)
			where THandler : IElementHandler, new()
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
			where THandler : IElementHandler, new()
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

		protected Task OnLoadedAsync(VisualElement frameworkElement, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			var source = new TaskCompletionSource();
			if (frameworkElement.IsLoaded && frameworkElement.IsLoadedOnPlatform())
			{
				source.TrySetResult();
			}
			else
			{
				EventHandler loaded = null;

				loaded = (_, __) =>
				{
					if (loaded is not null)
						frameworkElement.Loaded -= loaded;

					source.TrySetResult();
				};

				frameworkElement.Loaded += loaded;
			}

			return HandleLoadedUnloadedIssue(source.Task, timeOut.Value, () => frameworkElement.IsLoaded && frameworkElement.IsLoadedOnPlatform());
		}

		protected Task OnUnloadedAsync(VisualElement frameworkElement, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			var source = new TaskCompletionSource();
			if (!frameworkElement.IsLoaded && !frameworkElement.IsLoadedOnPlatform())
			{
				source.TrySetResult();
			}
			// in the xplat code we switch Loaded to Unloaded if the window property is removed.
			// This will happen before the the control has been unloaded at the platform level.
			// This is most likely a bug.
			else if (frameworkElement.IsLoadedOnPlatform())
			{
				frameworkElement.OnUnloaded(() => source.TrySetResult());
			}
			else
			{
				EventHandler unloaded = null;

				unloaded = (_, __) =>
				{
					if (unloaded is not null)
						frameworkElement.Unloaded -= unloaded;

					source.TrySetResult();
				};

				frameworkElement.Unloaded += unloaded;
			}

			return HandleLoadedUnloadedIssue(source.Task, timeOut.Value, () => !frameworkElement.IsLoaded && !frameworkElement.IsLoadedOnPlatform());
		}

		// Modal Page's appear to currently not fire loaded/unloaded
		async Task HandleLoadedUnloadedIssue(Task task, TimeSpan timeOut, Func<bool> isConditionValid)
		{
			try
			{
				await task.WaitAsync(timeOut);
			}
			catch (TimeoutException)
			{
				if (isConditionValid())
				{
					return;
				}
				else
				{
					throw;
				}
			}
		}

		protected async Task OnNavigatedToAsync(Page page, TimeSpan? timeOut = null)
		{
			await OnLoadedAsync(page, timeOut);

			if (page.HasNavigatedTo)
			{
				// TabbedPage fires OnNavigated earlier than it should
				if (page.Parent is TabbedPage)
					await Task.Delay(10);

				if (page is IPageContainer<Page> pc)
					await OnNavigatedToAsync(pc.CurrentPage);

				return;
			}

			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

			page.NavigatedTo += NavigatedTo;

			await taskCompletionSource.Task.WaitAsync(timeOut.Value);

			// TabbedPage fires OnNavigated earlier than it should
			if (page.Parent is TabbedPage)
				await Task.Delay(10);

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
				() =>
				{
					var size = frameworkElement.GetBoundingBox().Size;
					return size.Height > 0 && size.Width > 0;
				});

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


		protected IToolbar GetToolbar(IElementHandler handler)
		{
			return (handler.VirtualView as IWindowController)
						.Window
						.GetVisualTreeDescendants()
						.OfType<IToolbarElement>()
						.SingleOrDefault(x => x.Toolbar is not null)
						?.Toolbar;
		}

		protected Task ValidateHasColor<THandler>(IView view, Color color, Action action = null) =>
			ValidateHasColor(view, color, typeof(THandler), action);

		protected static void MockAccessibilityExpectations(View view)
		{
#if IOS || MACCATALYST
			if (UIKit.UIAccessibility.IsVoiceOverRunning)
				return;

			var mapperOverride = view.GetRendererOverrides<IView>();

			mapperOverride.ModifyMapping(AutomationProperties.IsInAccessibleTreeProperty.PropertyName, (handler, virtualView, action) =>
			{
				if (virtualView is ILabel)
				{
					// accessibility for UILabel depends on if the text is set or not
					// so we want to make sure text has propagated to the platform view
					// before mocking accessibility expectations
					handler.UpdateValue(nameof(ILabel.Text));
				}
				(handler.PlatformView as UIKit.UIView)?.SetupAccessibilityExpectationIfVoiceOverIsOff();
				(mapperOverride as PropertyMapper).Chained[0]!.UpdateProperty(handler, view, nameof(AutomationProperties.IsInAccessibleTreeProperty));
			});
#endif
		}
	}
}
