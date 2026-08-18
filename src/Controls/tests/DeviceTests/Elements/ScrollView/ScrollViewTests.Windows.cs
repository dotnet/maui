using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using WControl = Microsoft.UI.Xaml.Controls.Control;
using WDependencyProperty = Microsoft.UI.Xaml.DependencyProperty;
using WSetter = Microsoft.UI.Xaml.Setter;
using WScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;
using WStyle = Microsoft.UI.Xaml.Style;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewTests
	{
		[Fact]
		public async Task EntryDoesNotReceiveFocusWhenWindowOpens()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var focused = false;
			entry.Focused += (_, _) => focused = true;

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children =
					{
						entry
					}
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitAssert(() => !platformScrollView.IsTabStop);

				for (int i = 0; i < 5; i++)
				{
					await Task.Delay(50);
					Assert.False(entry.IsFocused);
					Assert.False(focused);
				}
			});
		}

		[Fact]
		public async Task ScrollViewerPreservesNativeTabStopDefaultAcrossHandlerLifecycle()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
				});
			});

			await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollView();
				var handler = CreateHandler<ScrollViewHandler>(scrollView);
				var mauiContext = handler.MauiContext;
				var platformView = handler.PlatformView;
				var nativeDefault = new WScrollViewer().IsTabStop;

				Assert.True(platformView.IsTabStop);

				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(nativeDefault, platformView.IsTabStop);
				Assert.Same(WDependencyProperty.UnsetValue, platformView.ReadLocalValue(WControl.IsTabStopProperty));

				((IElementHandler)handler).SetMauiContext(mauiContext);
				((IElementHandler)handler).SetVirtualView(scrollView);
				var reconnectedPlatformView = handler.PlatformView;
				Assert.True(reconnectedPlatformView.IsTabStop);

				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(nativeDefault, reconnectedPlatformView.IsTabStop);
				Assert.Same(WDependencyProperty.UnsetValue, reconnectedPlatformView.ReadLocalValue(WControl.IsTabStopProperty));
			});
		}

		[Fact]
		public async Task ScrollViewerPreservesCustomTabStopValueAfterLoading()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, CustomTabStopScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children =
					{
						entry
					}
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitAssert(() => !platformScrollView.IsTabStop);

				Assert.Equal(false, platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
				Assert.False(entry.IsFocused);
			});
		}

		[Fact]
		public async Task ScrollViewerPreservesStyledTabStopValueAfterLoading()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, StyledTabStopScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children =
					{
						entry
					}
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitAssert(() =>
					ReferenceEquals(
						WDependencyProperty.UnsetValue,
						platformScrollView.ReadLocalValue(WControl.IsTabStopProperty)));

				Assert.True(platformScrollView.IsTabStop);
				Assert.False(entry.IsFocused);
			});
		}

		[Fact]
		public async Task EntryDoesNotReceiveFocusWhenScrollViewReloads()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<NavigationPage, NavigationViewHandler>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
					handlers.AddHandler<Toolbar, ToolbarHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var focused = false;
			entry.Focused += (_, _) => focused = true;

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children =
					{
						entry
					}
				}
			};
			var page = new ContentPage { Content = scrollView };
			var navigationPage = new NavigationPage(page);
			var window = new Window(navigationPage);

			await CreateHandlerAndAddToWindow<IWindowHandler>(window, async _ =>
			{
				var scrollViewHandler = (ScrollViewHandler)scrollView.Handler;
				var platformScrollView = scrollViewHandler.PlatformView;
				await WaitAssert(() => !platformScrollView.IsTabStop);

				var unloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				Microsoft.UI.Xaml.RoutedEventHandler unloadedHandler = (_, _) => unloaded.TrySetResult();
				platformScrollView.Unloaded += unloadedHandler;
				try
				{
					await navigationPage.PushAsync(new ContentPage { Content = new Label() });
					await unloaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
				}
				finally
				{
					platformScrollView.Unloaded -= unloadedHandler;
				}

				Assert.False(platformScrollView.IsLoaded);
				Assert.Same(scrollViewHandler, scrollView.Handler);

				var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				Microsoft.UI.Xaml.RoutedEventHandler loadedHandler = (_, _) => loaded.TrySetResult();
				platformScrollView.Loaded += loadedHandler;
				try
				{
					await navigationPage.PopAsync();
					await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
				}
				finally
				{
					platformScrollView.Loaded -= loadedHandler;
				}

				Assert.True(platformScrollView.IsLoaded);
				await WaitAssert(() => !platformScrollView.IsTabStop);

				for (int i = 0; i < 5; i++)
				{
					await Task.Delay(50);
					Assert.False(entry.IsFocused);
					Assert.False(focused);
				}
			});
		}

		[Fact]
		public async Task QueuedTabStopRestoreDoesNotRunAfterDisconnect()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<NavigationPage, NavigationViewHandler>();
					handlers.AddHandler<ScrollView, DisconnectOnLoadScrollViewHandler>();
					handlers.AddHandler<Toolbar, ToolbarHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children =
					{
						new Entry()
					}
				}
			};
			var page = new ContentPage { Content = scrollView };
			var navigationPage = new NavigationPage(page);

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(navigationPage), async _ =>
			{
				var handler = (DisconnectOnLoadScrollViewHandler)scrollView.Handler;
				var platformScrollView = handler.PlatformView;
				await WaitAssert(() => !platformScrollView.IsTabStop);

				var unloaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				Microsoft.UI.Xaml.RoutedEventHandler unloadedHandler = (_, _) => unloaded.TrySetResult();
				platformScrollView.Unloaded += unloadedHandler;
				try
				{
					await navigationPage.PushAsync(new ContentPage { Content = new Label() });
					await unloaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
				}
				finally
				{
					platformScrollView.Unloaded -= unloadedHandler;
				}

				Assert.False(platformScrollView.IsLoaded);
				handler.DisconnectOnNextLoad = true;
				handler.BeforeDisconnect = () => page.Content = null;
				await navigationPage.PopAsync();
				await handler.Disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5));
				await Task.Delay(100);

				Assert.Same(WDependencyProperty.UnsetValue, platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
			});
		}

		sealed class CustomTabStopScrollViewHandler : ScrollViewHandler
		{
			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.IsTabStop = false;
			}
		}

		sealed class StyledTabStopScrollViewHandler : ScrollViewHandler
		{
			protected override WScrollViewer CreatePlatformView()
			{
				var style = new WStyle(typeof(WScrollViewer));
				style.Setters.Add(new WSetter
				{
					Property = WControl.IsTabStopProperty,
					Value = true
				});

				return new WScrollViewer
				{
					Style = style
				};
			}
		}

		sealed class DisconnectOnLoadScrollViewHandler : ScrollViewHandler
		{
			public bool DisconnectOnNextLoad { get; set; }

			public Action BeforeDisconnect { get; set; }

			public TaskCompletionSource Disconnected { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			protected override void ConnectHandler(WScrollViewer platformView)
			{
				platformView.Loaded += OnLoaded;
				base.ConnectHandler(platformView);
			}

			protected override void DisconnectHandler(WScrollViewer platformView)
			{
				platformView.Loaded -= OnLoaded;
				base.DisconnectHandler(platformView);
			}

			void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
			{
				if (!DisconnectOnNextLoad)
					return;

				DisconnectOnNextLoad = false;
				if (PlatformView.DispatcherQueue?.TryEnqueue(() =>
				{
					BeforeDisconnect?.Invoke();
					((IElementHandler)this).DisconnectHandler();
					Disconnected.TrySetResult();
				}) != true)
				{
					Disconnected.TrySetException(new InvalidOperationException("Unable to queue handler disconnect."));
				}
			}
		}
	}
}
