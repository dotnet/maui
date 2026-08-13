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
				InputTransparent = true
			};

			await AttachAndRun(label, (LabelHandler handler) =>
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
			});
		}
	}
}
