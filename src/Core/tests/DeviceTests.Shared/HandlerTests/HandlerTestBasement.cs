using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public class HandlerTestBasement : TestBase, IAsyncDisposable
	{
		MauiApp _mauiApp;
		IServiceProvider _servicesProvider;
		IMauiContext _mauiContext;
		bool _isCreated;

		public void EnsureHandlerCreated(Action<MauiAppBuilder> additionalCreationActions = null)
		{
			if (_isCreated)
			{
				return;
			}

			_isCreated = true;


			var appBuilder = MauiApp.CreateBuilder();

			appBuilder.Services.AddSingleton<IDispatcherProvider>(svc => TestDispatcher.Provider);
			appBuilder.Services.AddKeyedSingleton<IDispatcher>(typeof(IApplication), (svc, key) => TestDispatcher.Current);
			appBuilder.Services.AddScoped<IDispatcher>(svc => TestDispatcher.Current);
			appBuilder.Services.AddSingleton<IApplication>((_) => new CoreApplicationStub());

			appBuilder = ConfigureBuilder(appBuilder);
			additionalCreationActions?.Invoke(appBuilder);

			_mauiApp = appBuilder.Build();
			_servicesProvider = _mauiApp.Services;

			_mauiContext = new ContextStub(_servicesProvider);
		}

		protected virtual MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
		{
			return mauiAppBuilder;
		}

		protected IMauiContext MauiContext
		{
			get
			{
				EnsureHandlerCreated();
				return _mauiContext;
			}
		}

		protected IServiceProvider ApplicationServices
		{
			get
			{
				EnsureHandlerCreated();
				return _servicesProvider;
			}
		}

		protected Task SetValueAsync<TValue, THandler>(IView view, TValue value, Action<THandler, TValue> func)
			where THandler : IElementHandler, new()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<THandler>(view);
				func(handler, value);
			});
		}

		protected THandler CreateHandler<THandler>(IElement view, IMauiContext mauiContext = null)
			where THandler : IElementHandler, new()
			=> CreateHandler<THandler, THandler>(view, mauiContext);

		protected void InitializeViewHandler(IElement element, IElementHandler handler, IMauiContext mauiContext = null)
		{
			mauiContext ??= MauiContext;
			handler.SetMauiContext(mauiContext);
			element.Handler = handler;
			handler.SetVirtualView(element);

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
#elif IOS || MACCATALYST
				var size = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
				var w = size.Width;
				var h = size.Height;

				// No measure method should be returning infinite values
				Assert.False(double.IsPositiveInfinity(w));
				Assert.False(double.IsPositiveInfinity(h));

#else
				// Windows cannot measure without the view being loaded
				// iOS needs more love when I get an IDE again
				var w = view.Width.Equals(double.NaN) ? -1 : view.Width;
				var h = view.Height.Equals(double.NaN) ? -1 : view.Height;
#endif

				view.Arrange(new Rect(0, 0, w, h));

#if WINDOWS
				if (viewHandler.PlatformView is SwipeControl swipeControl && !swipeControl.IsLoaded)
				{
					void SwipeViewLoaded(object s, RoutedEventArgs e)
					{
						swipeControl.Arrange(new global::Windows.Foundation.Rect(0, 0, w, h));
						swipeControl.Loaded -= SwipeViewLoaded;
					};

					// Doing the SwipeItems arrange before the view has loaded causes the SwipeControl
					// to crash on the first layout pass. So we wait until the control has been loaded.
					swipeControl.Loaded += SwipeViewLoaded;
				}
				else
#endif
				viewHandler.PlatformArrange(view.Frame);
			}
		}

		protected TCustomHandler CreateHandler<THandler, TCustomHandler>(IElement element, IMauiContext mauiContext)
			where THandler : IElementHandler, new()
			where TCustomHandler : THandler, new()
		{
			if (element.Handler is TCustomHandler t)
				return t;

			mauiContext ??= MauiContext;
			var handler = Activator.CreateInstance<TCustomHandler>();
			InitializeViewHandler(element, handler, mauiContext);
			return handler;
		}

		protected IPlatformViewHandler CreateHandler(IElement view, Type handlerType) =>
			CreateHandler(view, handlerType, MauiContext);

		protected IPlatformViewHandler CreateHandler(IElement view, Type handlerType, IMauiContext mauiContext)
		{
			if (view.Handler is IPlatformViewHandler t)
				return t;

			var handler = (IPlatformViewHandler)Activator.CreateInstance(handlerType);
			InitializeViewHandler(view, handler, mauiContext);
			return handler;

		}

		protected Task ValidateHasColor(
			IView view,
			Color color,
			Type handlerType,
			Action action = null,
			string updatePropertyValue = null,
			double? tolerance = null)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var mauiContext = view?.Handler?.MauiContext ?? MauiContext;
				var handler = CreateHandler(view, handlerType, mauiContext);
				var plaformView = handler.ToPlatform();
				action?.Invoke();
				if (!string.IsNullOrEmpty(updatePropertyValue))
				{
					handler.UpdateValue(updatePropertyValue);
				}

				await plaformView.AssertContainsColor(color, mauiContext, tolerance: tolerance);
			});
		}

		public Task AttachAndRun(IView view, Action<IPlatformViewHandler> action) =>
				AttachAndRun<bool>(view, (handler) =>
				{
					action(handler);
					return Task.FromResult(true);
				});

		public Task AttachAndRun(IView view, Func<IPlatformViewHandler, Task> action) =>
				AttachAndRun<bool>(view, async (handler) =>
				{
					await action(handler);
					return true;
				});

		public Task<T> AttachAndRun<T>(IView view, Func<IPlatformViewHandler, T> action)
		{
			Func<IPlatformViewHandler, Task<T>> boop = (handler) =>
			{
				return Task.FromResult(action.Invoke(handler));
			};

			return AttachAndRun<T>(view, boop);
		}

		public Task<T> AttachAndRun<T>(IView view, Func<IPlatformViewHandler, Task<T>> action)
		{
			return view.AttachAndRun<T, IPlatformViewHandler>((handler) =>
			{
				return action.Invoke((IPlatformViewHandler)handler);
			}, MauiContext, (view) =>
			{
				if (view.Handler is IPlatformViewHandler platformViewHandler)
					return Task.FromResult(platformViewHandler);

				var handler = view.ToHandler(MauiContext);
				InitializeViewHandler(view, handler, MauiContext);
				return Task.FromResult(handler);
			});
		}

		public Task AttachAndRun<TPlatformHandler>(IView view, Func<TPlatformHandler, Task> action)
			where TPlatformHandler : IPlatformViewHandler, IElementHandler, new()
			=>
			view.AttachAndRun<bool, TPlatformHandler>(async (handler) =>
			{
				await action(handler);
				return true;
			}, MauiContext, async (view) =>
			{
				var result = await InvokeOnMainThreadAsync(() => CreateHandler<TPlatformHandler>(view));
				return result;
			});

		public Task AttachAndRun<TPlatformHandler>(IView view, Action<TPlatformHandler> action)
			where TPlatformHandler : IPlatformViewHandler, IElementHandler, new()
			=>
			view.AttachAndRun<bool, TPlatformHandler>((handler) =>
			{
				action(handler);
				return true;
			}, MauiContext, async (view) =>
			{
				var result = await InvokeOnMainThreadAsync(() => CreateHandler<TPlatformHandler>(view));
				return result;
			});

		protected Task AssertColorAtPoint(IView view, Color color, Type handlerType, int x, int y)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var plaformView = CreateHandler(view, handlerType).ToPlatform();
#if WINDOWS
				await plaformView.AssertColorAtPointAsync(color.ToWindowsColor(), x, y, MauiContext);
#else
				await plaformView.AssertColorAtPointAsync(color.ToPlatform(), x, y, MauiContext);
#endif
			});
		}

		protected Task AssertColorsAtPoints(IView view, Type handlerType, Color[] colors, Point[] points)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var plaformView = CreateHandler(view, handlerType).ToPlatform();
				await plaformView.AssertColorsAtPointsAsync(colors, points, MauiContext);
			});
		}

		protected Task<ImageAnalysis.RawBitmap> GetRawBitmap(Controls.VisualElement view, Type handlerType)
		{
			return InvokeOnMainThreadAsync<RawBitmap>(async () =>
			{
				var platformView = CreateHandler(view, handlerType).ToPlatform();
#if WINDOWS
				return await platformView.AttachAndRun<RawBitmap>(async (window) => await view.AsRawBitmapAsync(), MauiContext);
#else
				return await platformView.AttachAndRun<RawBitmap>(async () => await view.AsRawBitmapAsync());
#endif
			});
		}

		public async ValueTask DisposeAsync()
		{
			if (_mauiApp != null)
			{
				await ((IAsyncDisposable)_mauiApp).DisposeAsync();
			}

			_mauiApp = null;
			_servicesProvider = null;
			_mauiContext = null;
		}
	}
}
