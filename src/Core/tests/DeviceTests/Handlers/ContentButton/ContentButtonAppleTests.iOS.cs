using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Primitives;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonAppleTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Fact]
		public async Task NativeContentIsPresentationOnlyAndUpdates()
		{
			var first = new LabelStub { Text = "First" };
			var second = new LabelStub { Text = "Second" };
			var button = new ContentButtonStub { Content = first };

			var original = await InvokeOnMainThreadAsync(() =>
			{
				var view = first.ToPlatform(MauiContext);
				return (view.UserInteractionEnabled, view.IsAccessibilityElement, view.AccessibilityElementsHidden);
			});

			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var container = platformView.ContentContainerView;
				Assert.Same(platformView, container.Superview);
				Assert.False(container.UserInteractionEnabled);
				Assert.False(container.IsAccessibilityElement);
				Assert.True(container.AccessibilityElementsHidden);

				var firstPlatformView = Assert.IsAssignableFrom<UIView>(first.Handler?.PlatformView);
				Assert.Same(container, firstPlatformView.Superview);
				AssertOriginalState(firstPlatformView);

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				var secondPlatformView = Assert.IsAssignableFrom<UIView>(second.Handler?.PlatformView);
				Assert.Same(container, secondPlatformView.Superview);
				Assert.Null(firstPlatformView.Superview);
				AssertOriginalState(firstPlatformView);

				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));
				Assert.Null(secondPlatformView.Superview);
			});

			void AssertOriginalState(UIView view)
			{
				Assert.Equal(original.UserInteractionEnabled, view.UserInteractionEnabled);
				Assert.Equal(original.IsAccessibilityElement, view.IsAccessibilityElement);
				Assert.Equal(original.AccessibilityElementsHidden, view.AccessibilityElementsHidden);
			}
		}

		[Theory]
		[InlineData(false, 30, 0, 0, 0)]
		[InlineData(true, 30, 0, 0, 0)]
		[InlineData(true, 0, 30, 0, 0)]
		[InlineData(true, 10, 30, 0, 0)]
		[InlineData(true, 0, 0, 0, 0)]
		[InlineData(false, 10, 30, 5, 15)]
		[InlineData(true, 10, 30, 5, 15)]
		public async Task SimulatedSafeAreaInsetsRemainOnPhysicalEdges(
			bool rtl, double left, double right, double paddingLeft, double paddingRight)
		{
			var content = new LabelStub { Text = "Content" };
			var button = new ContentButtonStub
			{
				Content = content,
				Padding = new Thickness(paddingLeft, 0, paddingRight, 0),
			};

			await AttachAndRun(button, handler =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				var container = Assert.IsAssignableFrom<MauiView>(platformView.ContentContainerView);
				platformView.Frame = new CoreGraphics.CGRect(0, 0, 200, 100);
				platformView.SetNeedsLayout();
				platformView.LayoutIfNeeded();
				container.CellSafeAreaOverride = new SafeAreaPadding(left, right, 0, 0);
				container.SemanticContentAttribute = rtl
					? UISemanticContentAttribute.ForceRightToLeft
					: UISemanticContentAttribute.ForceLeftToRight;
				Assert.Equal(rtl
					? UIUserInterfaceLayoutDirection.RightToLeft
					: UIUserInterfaceLayoutDirection.LeftToRight,
					container.EffectiveUserInterfaceLayoutDirection);
				container.SetNeedsLayout();
				container.LayoutIfNeeded();

				var nativeContent = Assert.IsAssignableFrom<UIView>(content.Handler.PlatformView);
				Assert.Equal(left + (rtl ? paddingRight : paddingLeft), (double)nativeContent.Frame.X, 2);
				Assert.Equal(200 - right - (rtl ? paddingLeft : paddingRight),
					(double)(nativeContent.Frame.X + nativeContent.Frame.Width), 2);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task CellSafeAreaLookupUsesContentContainer()
		{
			var handler = await CreateHandlerAsync(new ContentButtonStub { Content = new LabelStub { Text = "Content" } });

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				Assert.Same(platformView.ContentContainerView, MauiView.GetCellSafeAreaView(platformView));

				// Shadows and clips wrap the button, as CollectionView cells see it.
				using var wrapper = new WrapperView();
				wrapper.AddSubview(platformView);
				Assert.Same(platformView.ContentContainerView, MauiView.GetCellSafeAreaView(wrapper));
				platformView.RemoveFromSuperview();
			});
		}

		[Fact]
		public async Task ContentReplacedWhileDetachingPreviousParentKeepsNewestContent()
		{
			var second = new LabelStub { Text = "Second" };
			var third = new LabelStub { Text = "Third" };
			var button = new ContentButtonStub { Content = new LabelStub { Text = "First" } };
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = Assert.IsType<MauiContentButton>(handler.PlatformView);
				using var host = new RemovalCallbackView();
				var secondView = second.ToPlatform(MauiContext);
				host.AddSubview(secondView);

				var replaced = false;
				host.SubviewRemoving = () =>
				{
					if (replaced)
						return;

					// A synchronous UIKit callback replaces the content while it is being re-parented.
					replaced = true;
					button.Content = third;
					handler.UpdateValue(nameof(IContentView.Content));
				};

				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));

				Assert.True(replaced);
				Assert.Same(third.Handler?.PlatformView, Assert.Single(platformView.ContentContainerView.Subviews));
				Assert.Null(secondView.Superview);
			});
		}

		sealed class RemovalCallbackView : UIView
		{
			internal Action SubviewRemoving;

			public override void WillRemoveSubview(UIView uiview)
			{
				base.WillRemoveSubview(uiview);
				SubviewRemoving?.Invoke();
			}
		}

		[Fact]
		public async Task LiveVirtualViewReplacementReleasesPreviousPress()
		{
			var firstPressed = 0;
			var firstReleased = 0;
			var secondPressed = 0;
			var secondReleased = 0;
			var first = new ContentButtonStub { Pressed = () => firstPressed++, Released = () => firstReleased++ };
			var second = new ContentButtonStub { Pressed = () => secondPressed++, Released = () => secondReleased++ };
			var handler = await CreateHandlerAsync(first);

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.SetVirtualView(first);
				Assert.Equal(1, firstPressed);
				Assert.Equal(0, firstReleased);

				handler.SetVirtualView(second);
				Assert.Equal(1, firstReleased);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside);
				Assert.Equal(0, secondReleased);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside);
				Assert.Equal(1, secondPressed);
				Assert.Equal(1, secondReleased);
				Assert.Equal(1, firstPressed);
			});
		}

		[Fact]
		public async Task TouchActivationDoesNotClickReplacementView()
		{
			var firstClicks = 0;
			var secondClicks = 0;
			var second = new ContentButtonStub { Clicked = () => secondClicks++ };
			var first = new ContentButtonStub { Clicked = () => firstClicks++ };
			var handler = await CreateHandlerAsync(first);
			first.Pressed = () => handler.SetVirtualView(second);

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				Assert.True(secondClicks == 0, "TouchDown should not click the replacement.");

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);

				Assert.Equal(0, firstClicks);
				Assert.True(secondClicks == 0, "The touch activation should not click the replacement.");

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, secondClicks);
			});
		}

		[Fact]
		public async Task CollectedTouchTargetDoesNotClickReplacementView()
		{
			var clicks = 0;
			var replacement = new ContentButtonStub { Clicked = () => clicks++ };
			var (original, handler) = await PressAndReplace(replacement);

			await AssertionExtensions.WaitForGC(original);
			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(0, clicks);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, clicks);
			});
			GC.KeepAlive(handler);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		async Task<(WeakReference Original, ContentButtonHandler Handler)> PressAndReplace(ContentButtonStub replacement)
		{
			var original = new ContentButtonStub();
			var handler = await CreateHandlerAsync(original);
			var reference = new WeakReference(original);

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.SetVirtualView(replacement);
			});

			return (reference, handler);
		}

		[Fact]
		public async Task ReentrantReplacementKeepsNewestView()
		{
			var firstReleased = 0;
			var first = new ContentButtonStub();
			var second = new ContentButtonStub();
			var third = new ContentButtonStub();
			var handler = await CreateHandlerAsync(first);

			await InvokeOnMainThreadAsync(() =>
			{
				first.Released = () =>
				{
					firstReleased++;
					handler.SetVirtualView(third);
				};
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);

				handler.SetVirtualView(second);

				Assert.Same(third, ((IElementHandler)handler).VirtualView);
				Assert.Equal(1, firstReleased);
			});
		}

		[Fact]
		public async Task NativeButtonRaisesWindowLifecycleEvents()
		{
			var handler = await CreateHandlerAsync(new ContentButtonStub());

			await InvokeOnMainThreadAsync(() =>
			{
				var events = 0;
				var lifecycle = Assert.IsAssignableFrom<IUIViewLifeCycleEvents>(handler.PlatformView);
				void OnMovedToWindow(object sender, System.EventArgs e) => events++;
				lifecycle.MovedToWindow += OnMovedToWindow;

#pragma warning disable CA1422 // A detached test window is sufficient to observe MovedToWindow.
				var window = new UIWindow(new CoreGraphics.CGRect(0, 0, 100, 100));
#pragma warning restore CA1422
				window.AddSubview(handler.PlatformView);
				handler.PlatformView.RemoveFromSuperview();
				lifecycle.MovedToWindow -= OnMovedToWindow;

				Assert.Equal(2, events);
			});
		}

		[Fact]
		public async Task HandlerProvidedNativeStylingIsRestoredAsDefault()
		{
			var button = new ContentButtonStub
			{
				Background = new SolidPaint(Colors.Blue),
				StrokeColor = Colors.Green,
				StrokeThickness = 6,
				CornerRadius = 9,
			};

			var handler = await InvokeOnMainThreadAsync(() =>
				CreateHandler<ContentButtonHandler, StyledContentButtonHandler>(button, MauiContext));

			await InvokeOnMainThreadAsync(() =>
			{
				button.Background = null;
				button.StrokeColor = null;
				button.StrokeThickness = -1;
				button.CornerRadius = -1;
				handler.UpdateValue(nameof(IView.Background));
				handler.UpdateValue(nameof(IButtonStroke.StrokeColor));
				handler.UpdateValue(nameof(IButtonStroke.StrokeThickness));
				handler.UpdateValue(nameof(IButtonStroke.CornerRadius));

				var layer = handler.PlatformView.Layer;
				Assert.Equal(StyledContentButtonHandler.Background, handler.PlatformView.BackgroundColor);
				Assert.Equal(StyledContentButtonHandler.Border.CGColor, layer.BorderColor);
				Assert.Equal(3, layer.BorderWidth);
				Assert.Equal(5, layer.CornerRadius);
			});
		}

		[Fact]
		public async Task StrokeUpdatesPreserveBackgroundMapperCustomization()
		{
			var button = new ContentButtonStub();
			var mapper = new PropertyMapper<IContentButton, IContentButtonHandler>(ContentButtonHandler.Mapper);
			var backgroundUpdates = 0;
			mapper.AppendToMapping(nameof(IView.Background), (handler, _) =>
			{
				backgroundUpdates++;
				handler.PlatformView.BackgroundColor = UIColor.Purple;
			});
			var handler = new ContentButtonHandler(mapper);

			await InvokeOnMainThreadAsync(() =>
			{
				InitializeViewHandler(button, handler);

				foreach (var property in new[] { nameof(IButtonStroke.StrokeColor), nameof(IButtonStroke.StrokeThickness), nameof(IButtonStroke.CornerRadius) })
				{
					var previousUpdates = backgroundUpdates;
					handler.UpdateValue(property);
					Assert.True(backgroundUpdates > previousUpdates);
					Assert.Equal(UIColor.Purple, handler.PlatformView.BackgroundColor);
				}
			});
		}

		sealed class StyledContentButtonHandler : ContentButtonHandler
		{
			internal static readonly UIColor Background = UIColor.Red;
			internal static readonly UIColor Border = UIColor.Purple;

			protected override UIButton CreatePlatformView()
			{
				var button = base.CreatePlatformView();
				button.BackgroundColor = Background;
				button.Layer.BorderColor = Border.CGColor;
				button.Layer.BorderWidth = 3;
				button.Layer.CornerRadius = 5;
				return button;
			}
		}

		[Fact]
		public async Task LayoutImplementingButtonDoesNotUseContentAsAccessibleName()
		{
			var content = new LabelStub { Text = "Presentation" };
			var button = new LayoutContentButtonStub { Content = content, Semantics = new Semantics { Hint = "Hint" } };
			button.Add(content);
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.True(string.IsNullOrEmpty(handler.PlatformView.AccessibilityLabel));
				Assert.Equal("Hint", handler.PlatformView.AccessibilityHint);
			});
		}

		sealed class LayoutContentButtonStub : LayoutStub, IContentButton
		{
			public IView Content { get; set; }
			object IContentView.Content => Content;
			public IView PresentedContent => Content;
			public Color StrokeColor { get; set; }
			public double StrokeThickness { get; set; } = -1;
			public int CornerRadius { get; set; } = -1;
			void IButton.Clicked() { }
			void IButton.Pressed() { }
			void IButton.Released() { }
		}

		[Fact]
		public async Task ReplacementPlatformViewIsRejected()
		{
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				InvokeOnMainThreadAsync(() =>
					CreateHandler<ContentButtonHandler, ReplacementContentButtonHandler>(new ContentButtonStub(), MauiContext)));

			Assert.Contains("base.CreatePlatformView()", exception.Message, StringComparison.Ordinal);
		}

		sealed class ReplacementContentButtonHandler : ContentButtonHandler
		{
			protected override UIButton CreatePlatformView() => new UIButton();
		}

		[Fact]
		public async Task NativeEventsHandleCancellationActivationDisableAndDisconnect()
		{
			var pressed = 0;
			var released = 0;
			var clicked = 0;
			var button = new ContentButtonStub
			{
				Pressed = () => pressed++,
				Released = () => released++,
				Clicked = () => clicked++,
			};

			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpOutside);
				Assert.Equal(1, pressed);
				Assert.Equal(1, released);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, clicked);

				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				button.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));
				Assert.Equal(2, pressed);
				Assert.Equal(2, released);
				Assert.False(handler.PlatformView.Enabled);

				button.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				((IElementHandler)handler).DisconnectHandler();
				Assert.Equal(3, pressed);
				Assert.Equal(3, released);

				platformView.SendActionForControlEvents(UIControlEvent.PrimaryActionTriggered);
				Assert.Equal(1, clicked);
			});
		}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1, 1)]
		[InlineData(100, 60)]
		[InlineData(double.PositiveInfinity, double.PositiveInfinity)]
		public async Task NativeMeasureHandlesTinyFiniteAndInfiniteConstraints(double width, double height)
		{
			var button = new ContentButtonStub
			{
				Width = Dimension.Unset,
				Height = Dimension.Unset,
				Content = new LabelStub { Text = "Content" },
				Padding = new Thickness(8),
				StrokeThickness = 2,
			};
			var handler = await CreateHandlerAsync(button);

			var size = await InvokeOnMainThreadAsync(() =>
				handler.PlatformView.SizeThatFits(new CoreGraphics.CGSize(width, height)));

			Assert.False(double.IsNaN(size.Width));
			Assert.False(double.IsNaN(size.Height));
			Assert.True(size.Width >= 0);
			Assert.True(size.Height >= 0);
		}

		[Fact]
		public async Task NativeMeasureIncludesContentAndInsets()
		{
			var button = new ContentButtonStub
			{
				Width = Dimension.Unset,
				Height = Dimension.Unset,
				Content = new LabelStub { Text = "Content", Width = 50, Height = 30 },
				Padding = new Thickness(8),
				StrokeThickness = 2,
			};
			var handler = await CreateHandlerAsync(button);

			var size = await InvokeOnMainThreadAsync(() =>
				handler.PlatformView.SizeThatFits(new CoreGraphics.CGSize(100, 80)));

			Assert.Equal(70, size.Width);
			Assert.Equal(50, size.Height);
		}

		[Fact]
		public async Task ExplicitWidthMeasuresContentAtArrangedWidthInNarrowParent()
		{
			var content = new LabelStub
			{
				Text = new string('W', 80),
				Width = Dimension.Unset,
				Height = Dimension.Unset,
			};
			var button = new ContentButtonStub
			{
				Width = 200,
				Height = Dimension.Unset,
				Content = content,
				Padding = new Thickness(5),
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var size = handler.GetDesiredSize(100, double.PositiveInfinity);

				Assert.Equal(200, size.Width, 1);
				Assert.Equal(190, content.DesiredSize.Width, 1);
			});
		}

		[Theory]
		[InlineData(80, 200, 500, 400, false, 200)]
		[InlineData(80, 200, 500, 400, true, 200)]
		[InlineData(200, 0, 500, 100, true, 200)]
		[InlineData(300, 0, 100, 400, false, 100)]
		[InlineData(300, 0, 100, 400, true, 100)]
		public async Task MeasurementUsesResolvedWidthForWrappedContent(double width, double minimumWidth, double maximumWidth, double parentWidth, bool wrapped, double expectedWidth)
		{
			var content = new LabelStub
			{
				Text = new string('W', 80),
				Width = Dimension.Unset,
				Height = Dimension.Unset,
			};
			var button = new ContentButtonStub
			{
				Width = width,
				MinimumWidth = minimumWidth,
				MaximumWidth = maximumWidth,
				Height = Dimension.Unset,
				Content = content,
				Padding = new Thickness(5),
				Clip = wrapped ? new PathShapeStub() : null,
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var size = handler.GetDesiredSize(parentWidth, double.PositiveInfinity);

				Assert.Equal(expectedWidth, size.Width, 1);
				Assert.Equal(expectedWidth - 10, content.DesiredSize.Width, 1);
			});
		}

		[Fact]
		public async Task EnablingWrappedButtonRestoresContainerInteraction()
		{
			var button = new ContentButtonStub
			{
				Clip = new PathShapeStub(),
				IsEnabled = false,
				Content = new LabelStub { Text = "Content" },
			};
			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(() =>
			{
				var containerView = Assert.IsType<WrapperView>(handler.ContainerView);
				Assert.False(containerView.UserInteractionEnabled);
				Assert.False(handler.PlatformView.Enabled);

				button.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.True(containerView.UserInteractionEnabled);
				Assert.True(handler.PlatformView.Enabled);
			});
		}

		[Fact]
		public async Task DisablingDuringPressToleratesReleasedDisconnectingHandler()
		{
			var released = 0;
			var button = new ContentButtonStub();
			var handler = await CreateHandlerAsync(button);
			button.Released = () =>
			{
				released++;
				((IElementHandler)handler).DisconnectHandler();
			};

			await InvokeOnMainThreadAsync(() =>
			{
				handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchDown);
				button.IsEnabled = false;

				var exception = Record.Exception(() => handler.UpdateValue(nameof(IView.IsEnabled)));

				Assert.Null(exception);
				Assert.Equal(1, released);
				Assert.Null(((IElementHandler)handler).PlatformView);
			});
		}
	}
}
