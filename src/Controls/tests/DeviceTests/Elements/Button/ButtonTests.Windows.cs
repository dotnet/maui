#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using Controls = Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
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

		[Fact]
		public async Task ReconnectingContainerRestoresOpacityChangedWhileDisconnected()
		{
			var button = new Controls.Button
			{
				Text = "Test",
				Clip = new Controls.Shapes.EllipseGeometry(new Point(50, 50), 50, 50)
			};

			await AttachAndRun<ButtonHandler>(button, handler =>
			{
				var container = Assert.IsType<WrapperView>(handler.ContainerView);
				var parent = container.Parent;
				button.Opacity = 0;
				Assert.Equal(0d, container.Opacity);

				((IElementHandler)handler).DisconnectHandler();
				button.Opacity = 1;
				handler.SetVirtualView(button);

				Assert.Same(container, handler.ContainerView);
				Assert.Same(parent, container.Parent);
				Assert.Same(handler.PlatformView, container.Child);
				Assert.Equal(1d, container.Opacity);
				Assert.Equal(1d, handler.PlatformView.Opacity);
			});
		}

		[Fact]
		public async Task ReconnectingContainerRestoresDefaultsAfterRepeatedContainerMapping()
		{
			var button = new Controls.Button
			{
				Text = "Test",
				Clip = new Controls.Shapes.EllipseGeometry(new Point(50, 50), 50, 50)
			};

			await AttachAndRun<RepeatedContainerMappingButtonHandler>(button, handler =>
			{
				var container = Assert.IsType<WrapperView>(handler.ContainerView);
				button.IsVisible = false;
				button.Opacity = 0;
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Collapsed, container.Visibility);
				Assert.Equal(0d, container.Opacity);
				var previousMappingCount = handler.ContainerMappingCount;

				((IElementHandler)handler).DisconnectHandler();
				button.IsVisible = true;
				button.Opacity = 1;
				handler.SetVirtualView(button);

				Assert.True(handler.ContainerMappingCount >= previousMappingCount + 2);
				Assert.Same(container, handler.ContainerView);
				Assert.Same(handler.PlatformView, container.Child);
				Assert.Equal(Microsoft.UI.Xaml.Visibility.Visible, container.Visibility);
				Assert.Equal(1d, container.Opacity);
				Assert.Equal(1d, handler.PlatformView.Opacity);
			});
		}

		class RepeatedContainerMappingButtonHandler : ButtonHandler
		{
			public int ContainerMappingCount { get; private set; }

			public RepeatedContainerMappingButtonHandler() : base(CreateMapper())
			{
			}

			static IPropertyMapper CreateMapper()
			{
				var mapper = new PropertyMapper<IButton, RepeatedContainerMappingButtonHandler>(ButtonHandler.Mapper);
				mapper.AppendToMapping(nameof(IViewHandler.ContainerView), (handler, view) => handler.ContainerMappingCount++);
				mapper.AppendToMapping(nameof(IView.Clip), (handler, view) => handler.UpdateValue(nameof(IViewHandler.ContainerView)));
				return mapper;
			}
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
