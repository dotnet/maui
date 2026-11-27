using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using ColumnDefinition = Microsoft.Maui.Controls.ColumnDefinition;
using Grid = Microsoft.Maui.Controls.Grid;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		sealed class PlatformViewParentTrackingLabelHandler : LabelHandler
		{
			public int ConnectedPlatformViewCount { get; private set; }
			public bool ConnectedPlatformViewsWereUnparented { get; private set; } = true;

			protected override void ConnectHandler(TextBlock platformView)
			{
				base.ConnectHandler(platformView);
				ConnectedPlatformViewCount++;
				ConnectedPlatformViewsWereUnparented &= platformView.Parent is null;
			}
		}

		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		[Theory]
		[InlineData(-1)]
		[InlineData(200)]
		public async Task GridStarColumnsKeepNativeLabelWithinCell(double widthRequest)
		{
			SetupBuilder();

			var layout = new VerticalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = widthRequest
			};

			await AttachAndRun(layout, async (LayoutHandler handler) =>
			{
				(string Title, string Value)[] items =
				[
					("Motion", "2 - Run Monitor"),
					("Calls", "2 - Run Monitor"),
					("Speed", "3 - Selector"),
					("Lockouts", "0 - Automatic"),
					("Automatic", "2 - No Calls"),
					("Run Monitor", "1 - Non-Emergency"),
					("Selector", "1 - Selector lost")
				];
				var grids = new Grid[items.Length];
				var titles = new Label[items.Length];
				var values = new Label[items.Length];
				var layoutCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				bool rowsAdded = false;

				void OnLayoutUpdated(object sender, object args)
				{
					if (!rowsAdded)
						return;

					for (int i = 0; i < items.Length; i++)
					{
						if (grids[i].Handler?.PlatformView is not FrameworkElement { IsLoaded: true, ActualWidth: > 0 } ||
							values[i].Handler is not LabelHandler label || !label.PlatformView.IsLoaded ||
							label.PlatformView.ActualWidth <= 0 || values[i].Width <= 0 ||
							titles[i].Text != items[i].Title || values[i].Text != items[i].Value)
						{
							return;
						}
					}

					layoutCompleted.TrySetResult();
				}

				handler.PlatformView.LayoutUpdated += OnLayoutUpdated;
				try
				{
					for (int i = 0; i < items.Length; i++)
					{
						var title = new Label { FontSize = 16, VerticalTextAlignment = TextAlignment.Center };
						title.SetBinding(Label.TextProperty, "Title");
						var valueLabel = new Label
						{
							FontSize = 14,
							FontAttributes = FontAttributes.Bold,
							LineBreakMode = LineBreakMode.TailTruncation,
							VerticalTextAlignment = TextAlignment.Center
						};
						valueLabel.SetBinding(Label.TextProperty, "Value");
						var rowGrid = new Grid
						{
							Padding = 10,
							BindingContext = new { items[i].Title, items[i].Value },
							ColumnDefinitions =
							{
								new ColumnDefinition(new GridLength(0.6, GridUnitType.Star)),
								new ColumnDefinition(new GridLength(0.2, GridUnitType.Star)),
								new ColumnDefinition(new GridLength(0.6, GridUnitType.Star))
							}
						};
						rowGrid.Add(title);
						rowGrid.Add(valueLabel, 2, 0);
						grids[i] = rowGrid;
						titles[i] = title;
						values[i] = valueLabel;
						layout.Add(rowGrid);
					}

					rowsAdded = true;
					handler.PlatformView.InvalidateMeasure();
					handler.PlatformView.UpdateLayout();
					await layoutCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5));
				}
				finally
				{
					handler.PlatformView.LayoutUpdated -= OnLayoutUpdated;
				}

				var grid = grids[5];
				var value = values[5];
				var labelHandler = Assert.IsType<LabelHandler>(value.Handler);
				var cellWidth = (grid.Width - grid.Padding.HorizontalThickness) * 3 / 7;
				var expectedX = grid.Padding.Left + (grid.Width - grid.Padding.HorizontalThickness) * 4 / 7;

				for (int i = 0; i < items.Length; i++)
				{
					Assert.Equal(grid.Width, grids[i].Width, 1.0);
					Assert.Equal(value.X, values[i].X, 1.0);
				}

				Assert.Equal(expectedX, value.X, 1.0);
				Assert.InRange(labelHandler.PlatformView.ActualWidth, 1, cellWidth + 1);
				Assert.InRange(labelHandler.PlatformView.ActualWidth, 1, value.Width + 1);
				if (labelHandler.ContainerView is not null)
					Assert.InRange(labelHandler.ContainerView.ActualWidth, 1, cellWidth + 1);
				Assert.Equal(TextTrimming.CharacterEllipsis, labelHandler.PlatformView.TextTrimming);
				if (widthRequest > 0)
				{
					Assert.True(labelHandler.PlatformView.IsTextTrimmed);
				}
				else
				{
					Assert.False(labelHandler.PlatformView.IsTextTrimmed);
					Assert.InRange(((IView)value).DesiredSize.Width, 1, value.Width + 1);
				}
			});
		}

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
