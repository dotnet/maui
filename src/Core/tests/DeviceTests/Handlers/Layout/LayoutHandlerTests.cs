using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	[Category(TestCategory.Layout)]
	public partial class LayoutHandlerTests : HandlerTestBase<LayoutHandler, LayoutStub>
	{
		[Fact(DisplayName = "Empty layout")]
		public async Task EmptyLayout()
		{
			var layout = new LayoutStub();
			await ValidatePropertyInitValue(layout, () => layout.Children.Count, GetNativeChildCount, 0);
		}

		[Fact(DisplayName = "Handler view count matches layout view count")]
		public async Task HandlerViewCountMatchesLayoutViewCount()
		{
			var layout = new LayoutStub();

			layout.Add(new SliderStub());
			layout.Add(new SliderStub());

			await ValidatePropertyInitValue(layout, () => layout.Children.Count, GetNativeChildCount, 2);
		}

		[Fact(DisplayName = "Handler removes child from native layout")]
		public async Task HandlerRemovesChildFromNativeLayout()
		{
			var layout = new LayoutStub();
			var slider = new SliderStub();
			layout.Add(slider);

			var handler = await CreateHandlerAsync(layout);

			var count = await InvokeOnMainThreadAsync(() =>
			{
				handler.Remove(slider);
				return GetNativeChildCount(handler);
			});

			Assert.Equal(0, count);
		}

		[Fact(DisplayName = "Assign New Handler To Layout")]
		public async Task AssignNewHandlerToLayout()
		{
			var layout = new LayoutStub();
			var slider = new SliderStub();
			layout.Add(slider);

			await CreateHandlerAsync(layout);
			layout.Handler = null;
			var handler = await CreateHandlerAsync(layout);
			Assert.Equal(handler, layout.Handler);
		}


		[Fact]
		public async Task SwitchHandlerInMiddleOfTree()
		{
			var root = new LayoutStub();
			var middle = new LayoutStub();
			var leaf = new SliderStub();

			root.Add(middle);
			middle.Add(leaf);

			var rootHandler = await CreateHandlerAsync(root);
			var middleHandler = middle.Handler;

			Assert.Same(rootHandler.NativeView, GetNativeParent(middleHandler as INativeViewHandler));
			Assert.Same(middleHandler.NativeView, GetNativeParent(leaf.Handler as INativeViewHandler));

			// Change the middle handler
			middle.Handler = null;
			middleHandler = await CreateHandlerAsync(middle);
			
			// Check our assumptions
			Assert.Equal(1, GetNativeChildCount(rootHandler));
			Assert.Equal(1, GetNativeChildCount(middleHandler as LayoutHandler));

			Assert.Same(rootHandler.NativeView, GetNativeParent(middleHandler as INativeViewHandler));
			Assert.Same(middleHandler.NativeView, GetNativeParent(leaf.Handler as INativeViewHandler));
		}
	}
}