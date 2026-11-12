#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Xunit;
using Controls = Microsoft.Maui.Controls;
using MauiTextAlignment = Microsoft.Maui.TextAlignment;
using WRect = global::Windows.Foundation.Rect;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		[Theory]
		[InlineData(MauiTextAlignment.Start)]
		[InlineData(MauiTextAlignment.Center)]
		[InlineData(MauiTextAlignment.End)]
		public async Task NativeTextAlignmentCanBeChangedAndCleared(MauiTextAlignment alignment)
		{
			var button = new Controls.Button { Text = "First line\nSecond line" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var content = Assert.IsAssignableFrom<FrameworkElement>(platform.Content);
				var text = Assert.IsType<TextBlock>(platform.GetContent<TextBlock>());
				var originalHorizontal = platform.HorizontalContentAlignment;
				var originalVertical = platform.VerticalContentAlignment;
				var originalPanelHorizontal = content.HorizontalAlignment;
				var originalPanelVertical = content.VerticalAlignment;
				var originalTextAlignment = text.ReadLocalValue(TextBlock.TextAlignmentProperty);
				var horizontal = alignment switch
				{
					MauiTextAlignment.Start => HorizontalAlignment.Left,
					MauiTextAlignment.End => HorizontalAlignment.Right,
					_ => HorizontalAlignment.Center
				};
				var vertical = alignment switch
				{
					MauiTextAlignment.Start => VerticalAlignment.Top,
					MauiTextAlignment.End => VerticalAlignment.Bottom,
					_ => VerticalAlignment.Center
				};

				button.HorizontalTextAlignment = alignment;
				Assert.Equal(horizontal, platform.HorizontalContentAlignment);
				Assert.Equal(horizontal, content.HorizontalAlignment);
				Assert.Equal(originalVertical, platform.VerticalContentAlignment);
				Assert.Equal(originalPanelVertical, content.VerticalAlignment);

				button.VerticalTextAlignment = alignment;
				Assert.Equal(vertical, platform.VerticalContentAlignment);
				Assert.Equal(vertical, content.VerticalAlignment);
				button.ClearValue(Controls.Button.HorizontalTextAlignmentProperty);
				Assert.Equal(originalHorizontal, platform.HorizontalContentAlignment);
				Assert.Equal(originalPanelHorizontal, content.HorizontalAlignment);
				Assert.Equal(originalTextAlignment, text.ReadLocalValue(TextBlock.TextAlignmentProperty));
				Assert.Equal(vertical, platform.VerticalContentAlignment);

				button.Text = string.Empty;
				Assert.Equal(originalVertical, platform.VerticalContentAlignment);
				Assert.Equal(originalPanelVertical, content.VerticalAlignment);
				button.Text = "Restored title";
				Assert.Equal(vertical, platform.VerticalContentAlignment);
				button.ClearValue(Controls.Button.VerticalTextAlignmentProperty);
				Assert.Equal(originalVertical, platform.VerticalContentAlignment);
				Assert.Equal(originalPanelVertical, content.VerticalAlignment);
			});
		}

		[Fact]
		public async Task NativeAlignmentIsNotRewrittenWhenUnset()
		{
			var button = new Controls.Button { Text = "Initial title" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var content = Assert.IsAssignableFrom<FrameworkElement>(platform.Content);
				var text = Assert.IsType<TextBlock>(platform.GetContent<TextBlock>());
				platform.HorizontalContentAlignment = content.HorizontalAlignment = HorizontalAlignment.Right;
				platform.VerticalContentAlignment = content.VerticalAlignment = VerticalAlignment.Bottom;
				text.TextAlignment = Microsoft.UI.Xaml.TextAlignment.Right;
				button.Text = "Updated title";
				Assert.Equal(HorizontalAlignment.Right, platform.HorizontalContentAlignment);
				Assert.Equal(HorizontalAlignment.Right, content.HorizontalAlignment);
				Assert.Equal(VerticalAlignment.Bottom, platform.VerticalContentAlignment);
				Assert.Equal(VerticalAlignment.Bottom, content.VerticalAlignment);
				Assert.Equal(Microsoft.UI.Xaml.TextAlignment.Right, text.TextAlignment);
			});
		}

		[Fact]
		public async Task ClearingAlignmentRestoresLiveNativeStyle()
		{
			var button = new Controls.Button { Text = "Styled title" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				platform.Style = new Style(typeof(Button))
				{
					Setters =
					{
						new Setter { Property = Control.HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Right },
						new Setter { Property = Control.VerticalContentAlignmentProperty, Value = VerticalAlignment.Bottom }
					}
				};
				button.HorizontalTextAlignment = button.VerticalTextAlignment = MauiTextAlignment.Center;
				Assert.Equal(HorizontalAlignment.Center, platform.HorizontalContentAlignment);
				Assert.Equal(VerticalAlignment.Center, platform.VerticalContentAlignment);

				platform.Style = new Style(typeof(Button))
				{
					Setters =
					{
						new Setter { Property = Control.HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Left },
						new Setter { Property = Control.VerticalContentAlignmentProperty, Value = VerticalAlignment.Top }
					}
				};
				button.ClearValue(Controls.Button.HorizontalTextAlignmentProperty);
				button.ClearValue(Controls.Button.VerticalTextAlignmentProperty);
				Assert.Equal(HorizontalAlignment.Left, platform.HorizontalContentAlignment);
				Assert.Equal(VerticalAlignment.Top, platform.VerticalContentAlignment);
				Assert.Equal(DependencyProperty.UnsetValue, platform.ReadLocalValue(Control.HorizontalContentAlignmentProperty));
				Assert.Equal(DependencyProperty.UnsetValue, platform.ReadLocalValue(Control.VerticalContentAlignmentProperty));
			});
		}

		[Fact]
		public async Task ClearingAlignmentRestoresNativeBindings()
		{
			var button = new Controls.Button
			{
				Text = "Bound title",
				HorizontalTextAlignment = MauiTextAlignment.Center,
				VerticalTextAlignment = MauiTextAlignment.Center
			};
			var handler = await CreateHandlerAsync<BoundAlignmentButtonHandler>(button);
			await InvokeOnMainThreadAsync(async () =>
			{
				var platform = handler.PlatformView;
				Assert.Equal(HorizontalAlignment.Center, platform.HorizontalContentAlignment);
				Assert.Equal(VerticalAlignment.Center, platform.VerticalContentAlignment);
				handler.AlignmentSource.HorizontalAlignment = HorizontalAlignment.Left;
				handler.AlignmentSource.VerticalAlignment = VerticalAlignment.Top;
				button.ClearValue(Controls.Button.HorizontalTextAlignmentProperty);
				button.ClearValue(Controls.Button.VerticalTextAlignmentProperty);
				Assert.Same(handler.AlignmentSource, platform.GetBindingExpression(Control.HorizontalContentAlignmentProperty)?.ParentBinding.Source);
				Assert.Same(handler.AlignmentSource, platform.GetBindingExpression(Control.VerticalContentAlignmentProperty)?.ParentBinding.Source);
				await AssertHelpers.AssertEventually(() => platform.HorizontalContentAlignment == HorizontalAlignment.Left &&
					platform.VerticalContentAlignment == VerticalAlignment.Top);

				handler.AlignmentSource.HorizontalAlignment = HorizontalAlignment.Right;
				handler.AlignmentSource.VerticalAlignment = VerticalAlignment.Bottom;
				await AssertHelpers.AssertEventually(() => platform.HorizontalContentAlignment == HorizontalAlignment.Right &&
					platform.VerticalContentAlignment == VerticalAlignment.Bottom);
			});
		}

		[Fact]
		public async Task AlignmentPreservesCustomNativeContent()
		{
			var button = new Controls.Button { Text = "Initial title" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var originalContent = platform.Content;
				var originalHorizontal = platform.HorizontalContentAlignment;
				var originalVertical = platform.VerticalContentAlignment;
				button.HorizontalTextAlignment = MauiTextAlignment.End;
				button.VerticalTextAlignment = MauiTextAlignment.Start;
				var customContent = new TextBlock { Text = "Custom", TextAlignment = Microsoft.UI.Xaml.TextAlignment.Right };
				platform.Content = customContent;
				Assert.Same(customContent, platform.Content);
				Assert.Equal(originalHorizontal, platform.HorizontalContentAlignment);
				Assert.Equal(originalVertical, platform.VerticalContentAlignment);

				button.HorizontalTextAlignment = MauiTextAlignment.Center;
				Assert.Same(customContent, platform.Content);
				Assert.Equal(Microsoft.UI.Xaml.TextAlignment.Right, customContent.TextAlignment);
				Assert.Equal(originalHorizontal, platform.HorizontalContentAlignment);

				platform.Content = originalContent;
				Assert.Equal(HorizontalAlignment.Center, platform.HorizontalContentAlignment);
				Assert.Equal(VerticalAlignment.Top, platform.VerticalContentAlignment);
			});
		}

		[Theory]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Left, false)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Top, false)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Right, false)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Bottom, false)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Left, true)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Top, true)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Right, true)]
		[InlineData(Controls.Button.ButtonContentLayout.ImagePosition.Bottom, true)]
		public async Task ImageAndTextGroupRendersAtRequestedAlignment(Controls.Button.ButtonContentLayout.ImagePosition position, bool rtl)
		{
			var button = new Controls.Button
			{
				Text = "A\nBBBB",
				ImageSource = "red.png",
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 20,
				WidthRequest = 300,
				HeightRequest = 260,
				BorderWidth = 0,
				Padding = new Thickness(24, 12, 8, 20),
				FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
				ContentLayout = new Controls.Button.ButtonContentLayout(position, 8)
			};

			await AttachAndRun<ButtonHandler>(button, async handler =>
			{
				var platform = handler.PlatformView;
				var content = Assert.IsAssignableFrom<FrameworkElement>(platform.Content);
				var text = Assert.IsType<TextBlock>(platform.GetContent<TextBlock>());
				var image = Assert.IsType<Image>(platform.GetContent<Image>());
				await AssertHelpers.AssertEventually(() => image.Source is not null && image.ActualWidth > 0 && image.ActualHeight > 0);
				var alignments = new[] { MauiTextAlignment.Start, MauiTextAlignment.Center, MauiTextAlignment.End };
				foreach (var rightToLeft in new[] { rtl, !rtl })
				{
					button.FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
					foreach (var horizontal in alignments)
						foreach (var vertical in alignments)
						{
							button.HorizontalTextAlignment = horizontal;
							button.VerticalTextAlignment = vertical;
							platform.UpdateLayout();
							Assert.Equal(rightToLeft ? Microsoft.UI.Xaml.FlowDirection.RightToLeft : Microsoft.UI.Xaml.FlowDirection.LeftToRight, platform.FlowDirection);
							Assert.Equal(horizontal switch
							{
								MauiTextAlignment.Center => Microsoft.UI.Xaml.TextAlignment.Center,
								MauiTextAlignment.Start => rightToLeft ? Microsoft.UI.Xaml.TextAlignment.Right : Microsoft.UI.Xaml.TextAlignment.Left,
								_ => rightToLeft ? Microsoft.UI.Xaml.TextAlignment.Left : Microsoft.UI.Xaml.TextAlignment.Right
							}, text.TextAlignment);

							var bounds = GetAlignmentBounds(content, platform);
							var padding = platform.Padding;
							var border = platform.BorderThickness;
							var expectedX = padding.Left + border.Left +
								(platform.ActualWidth - padding.Left - padding.Right - border.Left - border.Right - bounds.Width) *
								(horizontal == MauiTextAlignment.Start ? 0 : horizontal == MauiTextAlignment.End ? 1 : 0.5);
							var expectedY = padding.Top + border.Top +
								(platform.ActualHeight - padding.Top - padding.Bottom - border.Top - border.Bottom - bounds.Height) *
								(vertical == MauiTextAlignment.Start ? 0 : vertical == MauiTextAlignment.End ? 1 : 0.5);
							// Bounds in the button's RTL coordinate space are already mirrored by WinUI.
							Assert.InRange(Math.Abs(bounds.X - expectedX), 0, 1);
							Assert.InRange(Math.Abs(bounds.Y - expectedY), 0, 1);

							var imageBounds = GetAlignmentBounds(image, content);
							var textBounds = GetAlignmentBounds(text, content);
							Assert.True(imageBounds.Width > 0 && imageBounds.Height > 0 && textBounds.Width > 0 && textBounds.Height > 0);
							var gap = position switch
							{
								Controls.Button.ButtonContentLayout.ImagePosition.Top => textBounds.Y - (imageBounds.Y + imageBounds.Height),
								Controls.Button.ButtonContentLayout.ImagePosition.Bottom => imageBounds.Y - (textBounds.Y + textBounds.Height),
								Controls.Button.ButtonContentLayout.ImagePosition.Left => textBounds.X - (imageBounds.X + imageBounds.Width),
								_ => imageBounds.X - (textBounds.X + textBounds.Width)
							};
							Assert.InRange(Math.Abs(gap - button.ContentLayout.Spacing), 0, 1);
						}
				}

				button.ClearValue(Controls.Button.HorizontalTextAlignmentProperty);
				button.ClearValue(Controls.Button.VerticalTextAlignmentProperty);
				button.Text = string.Empty;
				platform.UpdateLayout();
				var imageOnlyBounds = GetAlignmentBounds(image, platform);
				var imageOnlyHorizontal = platform.HorizontalContentAlignment;
				var imageOnlyVertical = platform.VerticalContentAlignment;
				button.HorizontalTextAlignment = MauiTextAlignment.Start;
				button.VerticalTextAlignment = MauiTextAlignment.End;
				platform.UpdateLayout();
				Assert.Equal(imageOnlyHorizontal, platform.HorizontalContentAlignment);
				Assert.Equal(imageOnlyVertical, platform.VerticalContentAlignment);
				Assert.Equal(imageOnlyBounds, GetAlignmentBounds(image, platform));

				button.Text = "Restored title";
				platform.UpdateLayout();
				Assert.Equal(HorizontalAlignment.Left, platform.HorizontalContentAlignment);
				Assert.Equal(VerticalAlignment.Bottom, platform.VerticalContentAlignment);
			});
		}

		static WRect GetAlignmentBounds(FrameworkElement element, UIElement relativeTo) =>
			element.TransformToVisual(relativeTo).TransformBounds(new WRect(0, 0, element.ActualWidth, element.ActualHeight));

		class BoundAlignmentButtonHandler : ButtonHandler
		{
			public FrameworkElement AlignmentSource { get; } = new Border
			{
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom
			};

			public BoundAlignmentButtonHandler()
			{
			}

			protected override Button CreatePlatformView()
			{
				var platform = base.CreatePlatformView();
				platform.SetBinding(Control.HorizontalContentAlignmentProperty, new Binding
				{
					Source = AlignmentSource,
					Path = new PropertyPath(nameof(FrameworkElement.HorizontalAlignment)),
					Mode = BindingMode.OneWay
				});
				platform.SetBinding(Control.VerticalContentAlignmentProperty, new Binding
				{
					Source = AlignmentSource,
					Path = new PropertyPath(nameof(FrameworkElement.VerticalAlignment)),
					Mode = BindingMode.OneWay
				});
				return platform;
			}
		}

		[Theory]
		[InlineData("#B2F9F9F9")]
		[InlineData("#B21A7F37")]
		public async Task InheritedStylePreservesTransparentBackgroundEndpoints(string hoverColorHex)
		{
			var hoverColor = Color.FromArgb(hoverColorHex);
			var pointerOver = new Controls.VisualState { Name = "PointerOver" };
			pointerOver.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualElement.BackgroundColorProperty,
				Value = hoverColor
			});
			var commonStates = new Controls.VisualStateGroup { Name = "CommonStates" };
			commonStates.States.Add(new Controls.VisualState { Name = "Normal" });
			commonStates.States.Add(pointerOver);
			var baseStyle = new Controls.Style(typeof(Controls.Button));
			baseStyle.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualElement.BackgroundColorProperty,
				Value = Color.FromArgb("#F9F9F9")
			});
			baseStyle.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualStateManager.VisualStateGroupsProperty,
				Value = new Controls.VisualStateGroupList { commonStates }
			});
			var transparentStyle = new Controls.Style(typeof(Controls.Button)) { BasedOn = baseStyle };
			transparentStyle.Setters.Add(new Controls.Setter
			{
				Property = Controls.VisualElement.BackgroundColorProperty,
				Value = Color.FromArgb("#00000000")
			});
			var button = new Controls.Button { Text = "Test", Style = transparentStyle };

			await AttachAndRun<ButtonHandler>(button, handler =>
			{
				var resources = handler.PlatformView.Resources;
				var expectedHoverColor = hoverColor.ToWindowsColor();
				var expectedTransparentColor = global::Windows.UI.Color.FromArgb(
					0, expectedHoverColor.R, expectedHoverColor.G, expectedHoverColor.B);
				Assert.True(Controls.VisualStateManager.GoToState(button, "Normal"));

				for (var transition = 0; transition < 3; transition++)
				{
					var transparentBrush = Assert.IsType<Microsoft.UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]);
					Assert.Equal((byte)0, transparentBrush.Color.A);
					Assert.True(Controls.VisualStateManager.GoToState(button, "PointerOver"));
					Assert.Equal(hoverColor, button.BackgroundColor);
					Assert.Equal(expectedTransparentColor, transparentBrush.Color);
					var hoverBrush = Assert.IsType<Microsoft.UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]);
					Assert.Equal(expectedHoverColor, hoverBrush.Color);

					Assert.True(Controls.VisualStateManager.GoToState(button, "Normal"));
					Assert.Equal(Color.FromArgb("#00000000"), button.BackgroundColor);
					Assert.Equal(expectedHoverColor, hoverBrush.Color);
					Assert.Equal(expectedTransparentColor,
						Assert.IsType<Microsoft.UI.Xaml.Media.SolidColorBrush>(resources["ButtonBackground"]).Color);
				}

				return Task.CompletedTask;
			});
		}

		Button GetPlatformButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task<string?> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).GetContent<TextBlock>()?.Text);
		}

		TextTrimming GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			(GetPlatformButton(buttonHandler).Content as FrameworkElement)!.GetFirstDescendant<TextBlock>()!.TextTrimming;

		Task<float> GetPlatformOpacity(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return (float)nativeView.Opacity;
			});
		}

		Task<bool> GetPlatformIsVisible(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}

		[Fact]
		[Description("The Opacity property of a Button should match with native Opacity")]
		public async Task VerifyButtonOpacityProperty()
		{
			var button = new Microsoft.Maui.Controls.Button
			{
				Opacity = 0.35f
			};
			var expectedValue = button.Opacity;

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

	}
}
