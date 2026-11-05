using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		sealed class PlatformViewParentTrackingLabelHandler : LabelHandler
		{
			public int ConnectedPlatformViewCount { get; private set; }
			public int SetupContainerCount { get; private set; }
			public bool ConnectedPlatformViewsWereUnparented { get; private set; } = true;

			protected override void SetupContainer()
			{
				SetupContainerCount++;
				base.SetupContainer();
			}

			protected override void ConnectHandler(TextBlock platformView)
			{
				base.ConnectHandler(platformView);
				ConnectedPlatformViewCount++;
				ConnectedPlatformViewsWereUnparented &= platformView.Parent is null;
			}
		}

		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		// Yes, this looks wrong (because ultimately, it is)
		// We're returning TextTrimming instead of the obviously more correct TextWrapping because
		// LineBreakMode is a fundamentally incorrect conflation of wrapping and trimming. 
		// But for now we have to preserve the old Forms behavior and make the tests pass, so
		// these tests will consider Windows's "LineBreakMode" to be it's text trimming mode
		TextTrimming GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).TextTrimming;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).MaxLines;

		Task<float> GetPlatformOpacity(LabelHandler labelHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformLabel(labelHandler);
				return (float)nativeView.Opacity;
			});
		}

		Task<bool> GetPlatformIsVisible(LabelHandler labelHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformLabel(labelHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}

		[Theory]
		[InlineData(false, true)]
		[InlineData(true, false)]
		public async Task ContainerHitTestingTracksInputTransparent(bool inputTransparent, bool expectedIsHitTestVisible)
		{
			var label = new Label
			{
				BackgroundColor = Colors.Transparent,
				InputTransparent = inputTransparent
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			await InvokeOnMainThreadAsync(() =>
			{
				var container = Assert.IsType<WrapperView>(handler.ContainerView);

				Assert.Equal(expectedIsHitTestVisible, container.IsHitTestVisible);
				Assert.Equal(expectedIsHitTestVisible, handler.PlatformView.IsHitTestVisible);
			});
		}

		[Fact]
		public async Task ContainerHitTestingUpdatesWhenInputTransparentChanges()
		{
			var label = new Label
			{
				BackgroundColor = Colors.Transparent
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			await InvokeOnMainThreadAsync(() =>
			{
				var container = Assert.IsType<WrapperView>(handler.ContainerView);

				label.InputTransparent = true;

				Assert.False(container.IsHitTestVisible);
				Assert.False(handler.PlatformView.IsHitTestVisible);

				label.InputTransparent = false;

				Assert.True(container.IsHitTestVisible);
				Assert.True(handler.PlatformView.IsHitTestVisible);
			});
		}

		[Fact]
		public async Task ContainerCreatedAfterInputTransparentIsNotHitTestVisible()
		{
			var label = new Label
			{
				InputTransparent = true
			};

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Null(handler.ContainerView);
				Assert.False(handler.PlatformView.IsHitTestVisible);

				label.BackgroundColor = Colors.Transparent;

				var container = Assert.IsType<WrapperView>(handler.ContainerView);
				Assert.False(container.IsHitTestVisible);
				Assert.False(handler.PlatformView.IsHitTestVisible);
			});
		}

		[Fact]
		public async Task ContainerHitTestingIsRestoredWhenHandlerReconnects()
		{
			var label = new Label
			{
				BackgroundColor = Colors.Transparent,
				InputTransparent = true,
				VerticalTextAlignment = TextAlignment.Center
			};

			await AttachAndRun<PlatformViewParentTrackingLabelHandler>(label, handler =>
			{
				var oldPlatformView = handler.PlatformView;
				var container = Assert.IsType<WrapperView>(handler.ContainerView);
				var parent = Assert.IsAssignableFrom<FrameworkElement>(container.Parent);

				((IElementHandler)handler).DisconnectHandler();
				handler.SetVirtualView(label);

				var reconnectedContainer = Assert.IsType<WrapperView>(handler.ContainerView);
				Assert.NotSame(oldPlatformView, handler.PlatformView);
				Assert.Same(container, reconnectedContainer);
				Assert.Same(parent, reconnectedContainer.Parent);
				Assert.Same(handler.PlatformView, reconnectedContainer.Child);
				Assert.False(reconnectedContainer.IsHitTestVisible);
				Assert.False(handler.PlatformView.IsHitTestVisible);
				Assert.True(double.IsNaN(handler.PlatformView.Height));
				Assert.Equal(Microsoft.UI.Xaml.VerticalAlignment.Center, handler.PlatformView.VerticalAlignment);
				Assert.Equal(2, handler.ConnectedPlatformViewCount);
				Assert.True(handler.ConnectedPlatformViewsWereUnparented);
			});
		}

		[Theory]
		[InlineData("Visibility")]
		[InlineData("Opacity")]
		[InlineData("Transform2D")]
		[InlineData("Projection3D")]
		[InlineData("Clip")]
		[InlineData("Shadow")]
		[InlineData("InputTransparent")]
		public Task ReconnectingContainerRestoresDefaultPropertyValues(string property) =>
			VerifyReconnectedContainerProperty(property, 0);

		[Theory]
		[InlineData("Visibility", 1)]
		[InlineData("Opacity", 1)]
		[InlineData("Opacity", 2)]
		[InlineData("Transform2D", 1)]
		[InlineData("Transform2D", 2)]
		[InlineData("Projection3D", 1)]
		[InlineData("Projection3D", 2)]
		[InlineData("Clip", 1)]
		[InlineData("Clip", 2)]
		[InlineData("Shadow", 1)]
		[InlineData("Shadow", 2)]
		[InlineData("InputTransparent", 1)]
		public Task ReconnectingContainerRestoresNonDefaultPropertyValues(string property, int value) =>
			VerifyReconnectedContainerProperty(property, value);

		async Task VerifyReconnectedContainerProperty(string property, int value)
		{
			var label = new Label
			{
				Text = "Reconnect",
				BackgroundColor = Colors.Transparent,
				WidthRequest = 100,
				HeightRequest = 50
			};

			await AttachAndRun<PlatformViewParentTrackingLabelHandler>(label, handler =>
			{
				SetProperty(1);
				var wrapper = Assert.IsType<WrapperView>(handler.ContainerView);
				var parent = wrapper.Parent;
				var oldPlatformView = handler.PlatformView;
				var setupCount = handler.SetupContainerCount;
				AssertProperty(1);

				((IElementHandler)handler).DisconnectHandler();
				SetProperty(value);
				AssertProperty(1);
				handler.SetVirtualView(label);

				Assert.Same(wrapper, handler.ContainerView);
				Assert.Same(parent, wrapper.Parent);
				Assert.NotSame(oldPlatformView, handler.PlatformView);
				Assert.Same(handler.PlatformView, wrapper.Child);
				Assert.Equal(setupCount, handler.SetupContainerCount);
				AssertProperty(value);

				void SetProperty(int state)
				{
					switch (property)
					{
						case "Visibility":
							label.IsVisible = state == 0;
							break;
						case "Opacity":
							label.Opacity = state == 0 ? 1 : 0.25 * state;
							break;
						case "Transform2D":
							label.TranslationX = 12 * state;
							label.TranslationY = 4 * state;
							label.Rotation = 10 * state;
							label.Scale = state == 0 ? 1 : 0.8;
							label.AnchorX = 0.25;
							label.AnchorY = 0.75;
							break;
						case "Projection3D":
							label.RotationX = 15 * state;
							break;
						case "Clip":
							label.Clip = state == 0 ? null : new Microsoft.Maui.Controls.Shapes.RectangleGeometry
							{
								Rect = new Rect(0, 0, 40 * state, 20)
							};
							break;
						case "Shadow":
							label.Shadow = state == 0 ? null : new Shadow
							{
								Brush = new SolidColorBrush(Colors.Black),
								Radius = 8 * state
							};
							break;
						case "InputTransparent":
							label.InputTransparent = state != 0;
							break;
						default:
							throw new System.ArgumentOutOfRangeException(nameof(property));
					}
				}

				void AssertProperty(int state)
				{
					switch (property)
					{
						case "Visibility":
							Assert.Equal(state == 0 ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed,
								wrapper.Visibility);
							break;
						case "Opacity":
							Assert.Equal(state == 0 ? 1 : 0.25 * state, wrapper.Opacity, 6);
							break;
						case "Transform2D":
							if (state == 0)
							{
								var origin = new global::Windows.Foundation.Point(0, 0);
								var xAxis = new global::Windows.Foundation.Point(1, 0);
								var yAxis = new global::Windows.Foundation.Point(0, 1);
								var transform = wrapper.RenderTransform;
								Assert.Equal(origin, transform?.TransformPoint(origin) ?? origin);
								Assert.Equal(xAxis, transform?.TransformPoint(xAxis) ?? xAxis);
								Assert.Equal(yAxis, transform?.TransformPoint(yAxis) ?? yAxis);
							}
							else
							{
								var transform = Assert.IsType<Microsoft.UI.Xaml.Media.CompositeTransform>(wrapper.RenderTransform);
								Assert.Equal(12 * state, transform.TranslateX, 6);
								Assert.Equal(4 * state, transform.TranslateY, 6);
								Assert.Equal(10 * state, transform.Rotation, 6);
								Assert.Equal(0.8, transform.ScaleX, 6);
								Assert.Equal(0.8, transform.ScaleY, 6);
								Assert.Equal(0.25, wrapper.RenderTransformOrigin.X, 6);
								Assert.Equal(0.75, wrapper.RenderTransformOrigin.Y, 6);
							}
							break;
						case "Projection3D":
							if (state == 0)
								Assert.Null(wrapper.Projection);
							else
								Assert.Equal(-15 * state,
									Assert.IsType<Microsoft.UI.Xaml.Media.PlaneProjection>(wrapper.Projection).RotationX, 6);
							break;
						case "Clip":
							if (state == 0)
								Assert.Null(wrapper.Clip);
							else
								Assert.Equal(new Rect(0, 0, 40 * state, 20),
									Assert.IsType<Microsoft.Maui.Controls.Shapes.RectangleGeometry>(wrapper.Clip).Rect);
							break;
						case "Shadow":
							if (state == 0)
								Assert.Null(wrapper.Shadow);
							else
								Assert.Equal(8 * state, Assert.IsType<Shadow>(wrapper.Shadow).Radius, 6);
							break;
						case "InputTransparent":
							Assert.Equal(state == 0, wrapper.IsHitTestVisible);
							break;
					}
				}
			});
		}

		[Fact]
		public async Task ReconnectingVisibleContainerRestoresShadowHostVisibility()
		{
			var label = new Label
			{
				Text = "Shadow visibility",
				BackgroundColor = Colors.Transparent,
				WidthRequest = 100,
				HeightRequest = 50,
				Shadow = new Shadow
				{
					Brush = new SolidColorBrush(Colors.Black),
					Radius = 8,
					Offset = new Point(4, 4)
				}
			};

			await AttachAndRun(label, async (LabelHandler handler) =>
			{
				var wrapper = Assert.IsType<WrapperView>(handler.ContainerView);
				var parent = wrapper.Parent;
				await AssertEventually(() => FindShadowHost() is not null);
				var originalHost = FindShadowHost();
				Assert.NotNull(originalHost);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Visible, originalHost.Visibility);

				label.IsVisible = false;
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Collapsed, wrapper.Visibility);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Collapsed, handler.PlatformView.Visibility);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Collapsed, originalHost.Visibility);

				var oldPlatformView = handler.PlatformView;
				var shadow = label.Shadow;
				((IElementHandler)handler).DisconnectHandler();
				label.IsVisible = true;
				handler.SetVirtualView(label);

				await AssertEventually(() => FindShadowHost() is not null);
				Assert.Same(wrapper, handler.ContainerView);
				Assert.Same(parent, wrapper.Parent);
				Assert.NotSame(oldPlatformView, handler.PlatformView);
				Assert.Same(handler.PlatformView, wrapper.Child);
				Assert.Same(shadow, label.Shadow);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Visible, wrapper.Visibility);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Visible, handler.PlatformView.Visibility);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Visible, FindShadowHost().Visibility);

				int canvasCount = 0;
				foreach (var child in wrapper.CachedChildren)
				{
					if (child is Canvas)
						canvasCount++;
				}
				Assert.Equal(1, canvasCount);

				UIElement FindShadowHost()
				{
					foreach (var child in wrapper.CachedChildren)
					{
						if (child is Canvas canvas)
						{
#pragma warning disable RS0030 // Inspect the native shadow host after asynchronous composition setup.
							var hosts = canvas.Children;
#pragma warning restore RS0030
							foreach (var host in hosts)
							{
								if (Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementChildVisual(host) is not null)
									return host;
							}
						}
					}

					return null;
				}
			});
		}

		[Fact]
		public async Task ReconnectingContainerPreservesWrapperInDifferentPanel()
		{
			var label = new Label
			{
				BackgroundColor = Colors.Transparent,
				InputTransparent = true
			};

			await AttachAndRun(label, (LabelHandler handler) =>
			{
				var container = Assert.IsType<WrapperView>(handler.ContainerView);
				var originalParent = Assert.IsAssignableFrom<Panel>(container.Parent);
				var newParent = new Microsoft.UI.Xaml.Controls.Grid();

#pragma warning disable RS0030 // Do not use banned APIs; MauiPanel might not be used everywhere.
				var originalChildren = originalParent is MauiPanel mauiPanel
					? mauiPanel.CachedChildren
					: originalParent.Children;
				var newParentChildren = newParent.Children;
#pragma warning restore RS0030 // Do not use banned APIs

				var originalIndex = originalChildren.IndexOf(container);
				Assert.InRange(originalIndex, 0, originalChildren.Count - 1);
				originalChildren.RemoveAt(originalIndex);
				originalChildren.Insert(originalIndex, newParent);
				newParentChildren.Add(container);
				Assert.Same(newParent, container.Parent);

				((IElementHandler)handler).DisconnectHandler();
				handler.SetVirtualView(label);

				Assert.Same(container, handler.ContainerView);
				Assert.Same(newParent, container.Parent);
				Assert.Same(handler.PlatformView, container.Child);
				Assert.Same(container, handler.PlatformView.Parent);
			});
		}

		[Fact]
		public async Task ReconnectingContainerPreservesDetachedWrapper()
		{
			var label = new Label
			{
				BackgroundColor = Colors.Transparent,
				InputTransparent = true
			};

			await AttachAndRun(label, (LabelHandler handler) =>
			{
				var container = Assert.IsType<WrapperView>(handler.ContainerView);
				var parent = Assert.IsAssignableFrom<Panel>(container.Parent);

#pragma warning disable RS0030 // Do not use banned APIs; MauiPanel might not be used everywhere.
				var children = parent is MauiPanel mauiPanel
					? mauiPanel.CachedChildren
					: parent.Children;
#pragma warning restore RS0030 // Do not use banned APIs

				children.Remove(container);
				Assert.Null(container.Parent);

				((IElementHandler)handler).DisconnectHandler();
				handler.SetVirtualView(label);

				Assert.Same(container, handler.ContainerView);
				Assert.Null(container.Parent);
				Assert.Same(handler.PlatformView, container.Child);
				Assert.Contains(handler.PlatformView, container.CachedChildren);
			});
		}

		[Fact]
		public async Task FormattedTextDoesNotHangOrCrashWhenLabelIsShownAfterBeingSetWhileHidden()
		{
			SetupBuilder();

			var label = new Label
			{
				IsVisible = false,
				WidthRequest = 30,
				HorizontalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var layout = new VerticalStackLayout { label };

			await AttachAndRun(layout, async (handler) =>
			{
				label.FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "Aa Bb Cc" },
						new Span { Text = "\n" },
					}
				};

				await Task.Delay(50);

				// Showing the label triggers a real arrange pass, which calls
				// RecalculateSpanPositions(). Before the fix, this would hang the UI thread and
				// eventually crash with an OutOfMemoryException because the label wraps to 3
				// lines ("Aa", "Bb", "Cc") at this width. Simply completing (instead of hanging or
				// throwing) proves the fix works.
				label.IsVisible = true;

				await Task.Delay(250);
			});
		}
	}
}
