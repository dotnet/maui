using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ImageButton)]
	public partial class ImageButtonHandlerTests : HandlerTestBase<ImageButtonHandler, ImageButtonStub>
	{
		[Fact(DisplayName = "Click event fires Correctly")]
		public async Task ClickEventFires()
		{
			var clicked = false;

			var button = new ImageButtonStub();
			button.Clicked += delegate
			{
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);
		}

		[Category(TestCategory.ImageButton)]
		public partial class ImageButtonImageHandlerTests : ImageHandlerTests<ImageButtonHandler, ImageButtonStub>
		{
			public override Task AnimatedSourceInitializesCorrectly(string filename, bool isAnimating)
			{
				return Task.CompletedTask;
			}
		}
	}
}