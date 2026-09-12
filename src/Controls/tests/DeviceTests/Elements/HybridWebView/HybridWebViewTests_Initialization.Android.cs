#nullable enable
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_Initialization
{
	[Fact]
	public Task DisconnectHandlerRemovesClippingContainer() =>
		RunTest(_ => { }, (handler, view) =>
		{
			var platformView = handler.PlatformView;
			var container = Assert.IsType<WrapperView>(handler.ContainerView);
			var parent = Assert.IsAssignableFrom<ViewGroup>(container.Parent);
			Assert.Same(container, platformView.Parent);
			Assert.True(container.ClipChildren);
			Assert.Null(platformView.ClipBounds);

			((IElementHandler)handler).DisconnectHandler();
			Assert.Null(container.Parent);
			Assert.Null(platformView.Parent);
			Assert.Equal(0, container.ChildCount);
			Assert.Null(handler.ContainerView);
			Assert.False(handler.HasContainer);

			handler.SetVirtualView(view);
			var newContainer = Assert.IsType<WrapperView>(handler.ContainerView);
			Assert.NotSame(container, newContainer);
			Assert.NotSame(platformView, handler.PlatformView);
			Assert.Same(newContainer, handler.PlatformView.Parent);
			Assert.True(newContainer.ClipChildren);
			Assert.Null(handler.PlatformView.ClipBounds);
			parent.AddView(newContainer);

			((IElementHandler)handler).DisconnectHandler();
			((IElementHandler)handler).DisconnectHandler();
			Assert.Null(newContainer.Parent);
			Assert.Equal(0, newContainer.ChildCount);
			Assert.Null(handler.ContainerView);
			Assert.False(handler.HasContainer);
		});
}
