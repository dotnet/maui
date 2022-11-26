using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewTests
	{
		[Fact]
		public async Task ScrollViewContentSizeSet()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<Entry, EntryHandler>(); }); });

			var scrollView = new ScrollView();
			var entry = new Entry() { Text = "In a ScrollView", HeightRequest = 10000 };
			scrollView.Content = entry;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				return CreateHandlerAsync<ScrollViewHandler>(scrollView);
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					Assert.Equal(10000, scrollViewHandler.PlatformView.ContentSize.Height);
				});
			});
		}
	}
}
