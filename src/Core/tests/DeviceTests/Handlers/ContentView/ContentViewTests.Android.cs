using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.ContentView
{
	public partial class ContentViewTests
	{
		[Fact(DisplayName = "DisconnectHandler Does Not Throw When Platform View Is Disposed")]
		public async Task DisconnectHandlerDoesNotThrowWhenPlatformViewIsDisposed()
		{
			var contentView = new ContentViewStub
			{
				Content = new LabelStub { Text = "Content" }
			};

			var handler = await CreateHandlerAsync(contentView);

			await InvokeOnMainThreadAsync(() =>
			{
				var platformView = handler.PlatformView;
				platformView.Dispose();
				Assert.Equal(IntPtr.Zero, platformView.Handle);

				var exception = Record.Exception(() => ((IElementHandler)handler).DisconnectHandler());

				Assert.Null(exception);
			});
		}
	}
}
