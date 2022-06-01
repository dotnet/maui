using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public partial class ScrollViewTests : HandlerTestBase
	{
		[Fact]
		public async Task TestContentSizeChangedHorizontal()
		{
			var view = new Label { IsPlatformEnabled = true, BackgroundColor = Colors.LightGreen, Text = "Hello", WidthRequest = 100, HeightRequest = 100 };
			var labelHandler = await CreateHandlerAsync<LabelHandler>(view);

			var scroll = new ScrollView
			{
				Orientation = ScrollOrientation.Vertical,
				WidthRequest = 100, HeightRequest = 100
			};

			var handler = await CreateHandlerAsync<ScrollViewHandler>(scroll);

			//await InvokeOnMainThreadAsync(() =>
			//{
				
			//});

			//var dest = new Rect(0, 0, 100, 100);

			//await InvokeOnMainThreadAsync(() => {
			//	(scroll as IView).Measure(dest.Width, dest.Height);
			//	(scroll as IView).Arrange(dest);
			//});
			
			

			bool changed = false;
			scroll.PropertyChanged += (sender, e) =>
			{
				switch (e.PropertyName)
				{
					case "ContentSize":
						changed = true;
						break;
				}
			};

			// Okay, so we need to move all these tests to device tests
			// but we also don't actually have a content size update happening. 

			// We should test whether the invalidatemeasure natively works if we don't have a fixed size
			// for the scrollview (just out of curiosity). 
			// But since we _have_ to have a ContentSize update (for binding purposes), even if things work fine
			// without a re-measure we'll need to update it. Presumably we could do that in the handler? latch on
			// to the native size change stuff and update ContentSize appropriately? 

			// Or alternatively, override invalidatemeasureinternal for ScrollView and get it there. 
			// That's probably fine, at least for now. 

#if WINDOWS
			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					scroll.Content = view;

					await Task.Delay(100);

					Assert.Equal(new Size(100, 100), scroll.ContentSize);

					view.HeightRequest = 200;

					await Task.Delay(100);
				});

			});
#endif

			//await InvokeOnMainThreadAsync(() =>
			//{
			//	view.HeightRequest = 200;
			//});

			Assert.True(changed);
			Assert.Equal(new Size(100, 200), scroll.ContentSize);
		}
	}
}
