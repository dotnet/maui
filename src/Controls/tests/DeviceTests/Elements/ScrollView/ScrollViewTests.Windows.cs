using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Dispatching;
using Xunit;
using WBinding = Microsoft.UI.Xaml.Data.Binding;
using WBindingMode = Microsoft.UI.Xaml.Data.BindingMode;
using WControl = Microsoft.UI.Xaml.Controls.Control;
using WDependencyProperty = Microsoft.UI.Xaml.DependencyProperty;
using WPropertyPath = Microsoft.UI.Xaml.PropertyPath;
using WScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;
using WSetter = Microsoft.UI.Xaml.Setter;
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
					handlers.AddHandler<ScrollView, TrackingTabStopScrollViewHandler>();
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
				var handler = (TrackingTabStopScrollViewHandler)scrollView.Handler;
				var platformScrollView = handler.PlatformView;
				await handler.TemporaryTabStopObserved.Task.WaitAsync(TimeSpan.FromSeconds(5));
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.False(platformScrollView.IsTabStop);

				Assert.False(entry.IsFocused);
				Assert.False(focused);
				Assert.Same(
					platformScrollView,
					Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(platformScrollView.XamlRoot));

				var searchRoot = Assert.IsAssignableFrom<Microsoft.UI.Xaml.UIElement>(
					platformScrollView.XamlRoot.Content);
				Assert.True(Microsoft.UI.Xaml.Input.FocusManager.TryMoveFocus(
					Microsoft.UI.Xaml.Input.FocusNavigationDirection.Next,
					new Microsoft.UI.Xaml.Input.FindNextElementOptions { SearchRoot = searchRoot }));
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.True(entry.IsFocused);
			});
		}

		[Fact]
		public async Task ShiftTabFromScrollViewerMovesToPreviousControl()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var previousButton = new Button { Text = "Previous" };
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { new Entry() }
				}
			};
			var content = new VerticalStackLayout
			{
				Children =
				{
					scrollView
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = content }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.Same(
					platformScrollView,
					Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(platformScrollView.XamlRoot));

				content.Children.Insert(0, previousButton);
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				var searchRoot = Assert.IsAssignableFrom<Microsoft.UI.Xaml.UIElement>(
					platformScrollView.XamlRoot.Content);
				Assert.True(Microsoft.UI.Xaml.Input.FocusManager.TryMoveFocus(
					Microsoft.UI.Xaml.Input.FocusNavigationDirection.Previous,
					new Microsoft.UI.Xaml.Input.FindNextElementOptions { SearchRoot = searchRoot }));
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.True(previousButton.IsFocused);
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

				Assert.Equal(nativeDefault, platformView.IsTabStop);
				Assert.Same(WDependencyProperty.UnsetValue, platformView.ReadLocalValue(WControl.IsTabStopProperty));

				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(nativeDefault, platformView.IsTabStop);
				Assert.Same(WDependencyProperty.UnsetValue, platformView.ReadLocalValue(WControl.IsTabStopProperty));

				((IElementHandler)handler).SetMauiContext(mauiContext);
				((IElementHandler)handler).SetVirtualView(scrollView);
				var reconnectedPlatformView = handler.PlatformView;
				Assert.Equal(nativeDefault, reconnectedPlatformView.IsTabStop);
				Assert.Same(WDependencyProperty.UnsetValue, reconnectedPlatformView.ReadLocalValue(WControl.IsTabStopProperty));

				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(nativeDefault, reconnectedPlatformView.IsTabStop);
				Assert.Same(WDependencyProperty.UnsetValue, reconnectedPlatformView.ReadLocalValue(WControl.IsTabStopProperty));
			});
		}

		[Fact]
		public async Task ScrollViewerPreservesTabStopBindingAfterLoading()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, BoundTabStopScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { new Entry() }
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var handler = (BoundTabStopScrollViewHandler)scrollView.Handler;
				var platformScrollView = handler.PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.False(platformScrollView.IsTabStop);
				Assert.NotNull(platformScrollView.GetBindingExpression(WControl.IsTabStopProperty));

				handler.Source.Value = true;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.True(platformScrollView.IsTabStop);
			});
		}

		[Fact]
		public async Task UnfocusPreservesTwoWayTabStopBinding()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, TwoWayBoundTabStopScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { entry }
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var handler = (TwoWayBoundTabStopScrollViewHandler)scrollView.Handler;
				var platformScrollView = handler.PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				var originalBinding = platformScrollView
					.GetBindingExpression(WControl.IsTabStopProperty)
					?.ParentBinding;

				Assert.True(platformScrollView.IsTabStop);
				Assert.True(handler.Source.Value);
				Assert.NotNull(originalBinding);

				var entryFocused = false;
				entry.Focused += (_, _) => entryFocused = true;
				Microsoft.Maui.Platform.ViewExtensions.Unfocus(platformScrollView, scrollView);
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.True(platformScrollView.IsTabStop);
				Assert.True(handler.Source.Value);
				Assert.Same(
					originalBinding,
					platformScrollView.GetBindingExpression(WControl.IsTabStopProperty)?.ParentBinding);
				Assert.False(entryFocused);
				Assert.False(entry.IsFocused);
				Assert.NotSame(
					platformScrollView,
					Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(platformScrollView.XamlRoot));
			});
		}

		[Fact]
		public async Task NativeTabStopWriteDuringLoadingWins()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, NativeTabStopWriteScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { new Entry() }
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.True(platformScrollView.IsTabStop);
				Assert.Equal(true, platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
			});
		}

		[Fact]
		public async Task UnfocusDuringLoadingRestoresOriginalTabStopState()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, UnfocusOnLoadScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { entry }
				}
			};
			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.False(platformScrollView.IsTabStop);
				Assert.Same(
					WDependencyProperty.UnsetValue,
					platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
				Assert.False(entry.IsFocused);
			});
		}

		[Fact]
		public async Task ExplicitEntryFocusDuringLoadingIsPreserved()
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
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { entry }
				}
			};
			scrollView.Loaded += (_, _) => entry.Focus();

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.False(platformScrollView.IsTabStop);
				Assert.True(entry.IsFocused);
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
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.False(platformScrollView.IsTabStop);
				Assert.Equal(false, platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
				Assert.False(entry.IsFocused);
			});
		}

		[Fact]
		public async Task ScrollViewerPreservesCustomTrueTabStopValueAfterLoading()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, TrueTabStopScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { entry }
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.True(platformScrollView.IsTabStop);
				Assert.Equal(true, platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
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
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.Same(
					WDependencyProperty.UnsetValue,
					platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
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
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.False(platformScrollView.IsTabStop);

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
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.False(platformScrollView.IsTabStop);

				Assert.False(entry.IsFocused);
				Assert.False(focused);
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
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);
				Assert.False(platformScrollView.IsTabStop);

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
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.Same(WDependencyProperty.UnsetValue, platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
			});
		}

		[Fact]
		public async Task RemovingScrollViewDuringLoadingDoesNotStrandTemporaryTabStop()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, RemoveDuringLoadingScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children = { new Entry() }
				}
			};
			var page = new ContentPage { Content = scrollView };

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(page), async _ =>
			{
				var handler = (RemoveDuringLoadingScrollViewHandler)scrollView.Handler;
				var platformScrollView = handler.PlatformView;

				await handler.Removed.Task.WaitAsync(TimeSpan.FromSeconds(5));
				await WaitForDispatcherIdle(platformScrollView.DispatcherQueue);

				Assert.False(platformScrollView.IsLoaded);
				Assert.False(platformScrollView.IsTabStop);
				Assert.Same(
					WDependencyProperty.UnsetValue,
					platformScrollView.ReadLocalValue(WControl.IsTabStopProperty));
			});
		}

		static Task WaitForDispatcherIdle(DispatcherQueue dispatcherQueue)
		{
			var idle = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			if (!dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => idle.TrySetResult()))
				idle.TrySetException(new InvalidOperationException("Unable to queue dispatcher idle callback."));

			return idle.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}

		sealed class TrackingTabStopScrollViewHandler : ScrollViewHandler
		{
			public TaskCompletionSource TemporaryTabStopObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.Loaded += OnLoaded;
			}

			protected override void DisconnectHandler(WScrollViewer platformView)
			{
				platformView.Loaded -= OnLoaded;
				base.DisconnectHandler(platformView);
			}

			void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
			{
				if (((WScrollViewer)sender).IsTabStop)
					TemporaryTabStopObserved.TrySetResult();
				else
					TemporaryTabStopObserved.TrySetException(
						new InvalidOperationException("The temporary ScrollViewer tab stop was not active during loading."));
			}
		}

		sealed class CustomTabStopScrollViewHandler : ScrollViewHandler
		{
			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.IsTabStop = false;
			}
		}

		sealed class TrueTabStopScrollViewHandler : ScrollViewHandler
		{
			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.IsTabStop = true;
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

		sealed class BoundTabStopScrollViewHandler : ScrollViewHandler
		{
			public BooleanSource Source { get; } = new();

			protected override WScrollViewer CreatePlatformView()
			{
				var scrollViewer = new WScrollViewer();
				scrollViewer.SetBinding(WControl.IsTabStopProperty, new WBinding
				{
					Mode = WBindingMode.OneWay,
					Path = new WPropertyPath(nameof(BooleanSource.Value)),
					Source = Source
				});
				return scrollViewer;
			}
		}

		sealed class TwoWayBoundTabStopScrollViewHandler : ScrollViewHandler
		{
			public BooleanSource Source { get; } = new() { Value = true };

			protected override WScrollViewer CreatePlatformView()
			{
				var scrollViewer = new WScrollViewer();
				scrollViewer.SetBinding(WControl.IsTabStopProperty, new WBinding
				{
					Mode = WBindingMode.TwoWay,
					Path = new WPropertyPath(nameof(BooleanSource.Value)),
					Source = Source
				});
				return scrollViewer;
			}
		}

		sealed class NativeTabStopWriteScrollViewHandler : ScrollViewHandler
		{
			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.Loaded += SetTabStop;
			}

			protected override void DisconnectHandler(WScrollViewer platformView)
			{
				platformView.Loaded -= SetTabStop;
				base.DisconnectHandler(platformView);
			}

			static void SetTabStop(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
			{
				((WScrollViewer)sender).IsTabStop = true;
			}
		}

		sealed class UnfocusOnLoadScrollViewHandler : ScrollViewHandler
		{
			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.Loaded += Unfocus;
			}

			protected override void DisconnectHandler(WScrollViewer platformView)
			{
				platformView.Loaded -= Unfocus;
				base.DisconnectHandler(platformView);
			}

			void Unfocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
			{
				Microsoft.Maui.Platform.ViewExtensions.Unfocus((WScrollViewer)sender, VirtualView);
			}
		}

		sealed class BooleanSource : INotifyPropertyChanged
		{
			bool _value;

			public bool Value
			{
				get => _value;
				set
				{
					if (_value == value)
						return;

					_value = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

		sealed class DisconnectOnLoadScrollViewHandler : ScrollViewHandler
		{
			public bool DisconnectOnNextLoad { get; set; }

			public Action BeforeDisconnect { get; set; }

			public TaskCompletionSource Disconnected { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			protected override void ConnectHandler(WScrollViewer platformView)
			{
				// Queue the disconnect before the production Loaded callback queues its restore.
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

		sealed class RemoveDuringLoadingScrollViewHandler : ScrollViewHandler
		{
			public TaskCompletionSource Removed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

			protected override void ConnectHandler(WScrollViewer platformView)
			{
				base.ConnectHandler(platformView);
				platformView.Loading += OnLoading;
			}

			protected override void DisconnectHandler(WScrollViewer platformView)
			{
				platformView.Loading -= OnLoading;
				base.DisconnectHandler(platformView);
			}

			void OnLoading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
			{
				var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(sender);
				if (parent is Microsoft.Maui.Platform.MauiPanel panel &&
					panel.CachedChildren.Remove(sender))
				{
					Removed.TrySetResult();
				}
				else
				{
					Removed.TrySetException(
						new InvalidOperationException(
							$"Unable to remove the ScrollViewer from its native parent during Loading. Parent: {parent?.GetType().FullName ?? "<null>"}."));
				}
			}
		}
	}
}
