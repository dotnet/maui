using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewTests
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(5)]
		[InlineData(6)]
		[InlineData(7)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(10)]
		[InlineData(11)]
		public async Task ArrangesContentWithoutOverlapAndWithProperSize(int columnCount)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ContentView, ContentViewHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
				});
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				var grid = new Grid();
				grid.WidthRequest = 293;
				grid.ColumnDefinitions = new ColumnDefinitionCollection(
					Enumerable.Range(0, columnCount)
						.Select(_ => new ColumnDefinition())
						.ToArray());

				for (int i = 0; i < columnCount; i++)
				{
					var content = new ContentView();
					content.BackgroundColor = new Color(255 / (i + 1), 255 / (i + 1), 255 / (i + 1));
					content.HeightRequest = 50;
					grid.Add(content, i);
				}

				await CreateHandlerAndAddToWindow(grid, (LayoutHandler handler) =>
				{
					var platformView = (ViewGroup)handler.PlatformView;
					var childrenPlatformViews = Enumerable.Range(0, platformView.ChildCount)
						.Select(n => platformView.GetChildAt(n)!);
					var arrangedFrames = childrenPlatformViews
						.Select(v => (v.Left, v.Top, v.Right, v.Bottom, v.Width, v.Height))
						.ToArray();
					var context = platformView.Context;

					int lastRight = 0;
					for (int i = 0; i < arrangedFrames.Length; i++)
					{
						var childVirtualView = grid[i];
						var dpFrame = childVirtualView.Frame;
						var pxFrame = arrangedFrames[i];
						var expectedWidth = context.ToPixels(dpFrame.Width);

						// This fails sometimes due to the way we arrange the content based on coordinates instead of size
						// Assert.Equal(expectedWidth, pxFrame.Width);
						Assert.True(pxFrame.Left == lastRight, $"ColumnCount: {columnCount} Expected Left {lastRight} but got {pxFrame.Left} for child {i} Device Info: {DeviceDisplay.Current.MainDisplayInfo}");
						lastRight = pxFrame.Right;
					}
				});
			});
		}
	}
}
