using System;
using System.Linq;
using System.Runtime.InteropServices;
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
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase : HandlerTestBase
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
			//mauiAppBuilder.Services.AddScoped(_ => new HideSoftInputOnTappedChangedManager());
			return mauiAppBuilder.ConfigureTestBuilder();
		}

		protected THandler CreateHandler<THandler>(IElement view)
			where THandler : IElementHandler, new()
		{
			return CreateHandler<THandler>(view, MauiContext);
		}

		protected Task<TValue> GetValueAsync<TValue, THandler>(IElement view, Func<THandler, TValue> func)
			 where THandler : IElementHandler, new()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				return func(handler);
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

		static SemaphoreSlim _takeOverMainContentSempahore = new SemaphoreSlim(1);
		protected Task CreateHandlerAndAddToWindow<THandler>(IElement view, Func<THandler, Task> action, IMauiContext mauiContext = null, TimeSpan? timeOut = null)
			where THandler : class, IElementHandler
		{
			mauiContext ??= MauiContext;

			if (System.Diagnostics.Debugger.IsAttached)
				timeOut ??= TimeSpan.FromHours(1);
			else
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

						if (content is VisualElement vc)
						{
							await OnLoadedAsync(vc);

							if (vc.Frame.Height < 0 && vc.Frame.Width < 0)
							{
								var batchTcs = new TaskCompletionSource();
								vc.BatchCommitted += OnBatchCommitted;
								await batchTcs.Task.WaitAsync(timeOut.Value);
								if (vc.Frame.Height < 0 && vc.Frame.Width < 0)
								{
									Assert.Fail($"{content} Failed to layout");
								}

								void OnBatchCommitted(object sender, Controls.Internals.EventArg<VisualElement> e)
								{
									vc.BatchCommitted -= OnBatchCommitted;
									batchTcs.SetResult();
								}
							}
						}

						// Gives time for the measure/layout pass to settle
						await Task.Yield();
						if (view is VisualElement veBeingTested)
							await OnLoadedAsync(veBeingTested);

#if !WINDOWS
						if (window is Window controlsWindow)
						{
							//if (!controlsWindow.IsActivated)
							//	window.Activated();
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
						//bool isActivated = controlsWindow?.IsActivated ?? false;
						//bool isDestroyed = controlsWindow?.IsDestroyed ?? false;

						//if (isActivated)
						//	window.Deactivated();

						//if (!isDestroyed)
						//	window.Destroying();
#endif

					}, mauiContext);
				}
				finally
				{
					_takeOverMainContentSempahore.Release();
				}
			});
		}

		protected async Task OnLoadedAsync(VisualElement frameworkElement, TimeSpan? timeOut = null)
		{
			timeOut = timeOut ?? TimeSpan.FromSeconds(2);
			var source = new TaskCompletionSource();
			if (frameworkElement.IsLoaded && frameworkElement.IsLoadedOnPlatform())
			{
				await Task.Delay(50);
				source.TrySetResult();
			}
			else
			{
				EventHandler loaded = null;

				loaded = async (_, __) =>
				{
					if (loaded is not null)
						frameworkElement.Loaded -= loaded;
					try
					{
						await Task.Yield();
						source.TrySetResult();
					}
					catch (Exception e)
					{
						source.SetException(e);
					}
				};

				frameworkElement.Loaded += loaded;
			}

			await HandleLoadedUnloadedIssue(source.Task, timeOut.Value, () => frameworkElement.IsLoaded && frameworkElement.IsLoadedOnPlatform());
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
	}
}
