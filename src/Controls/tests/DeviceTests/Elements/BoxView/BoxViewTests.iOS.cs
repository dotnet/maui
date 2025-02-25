﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using System.ComponentModel;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BoxViewTests
	{
		MauiShapeView GetNativeBoxView(ShapeViewHandler boxViewHandler) =>
			boxViewHandler.PlatformView;

		[Fact(DisplayName = "ShapeView Parts Keep Around")]
		public async Task ShapeViewPartsKeepAround()
		{
			var boxView = new BoxView()
			{
				HeightRequest = 100,
				WidthRequest = 200
			};

			await AttachAndRun<ShapeViewHandler>(boxView, async handler =>
			{
				var shapeView = GetNativeBoxView(handler);
				var renderer = (DirectRenderer)shapeView.Renderer;

				GC.Collect();
				GC.WaitForPendingFinalizers();
				await Task.Yield();

				GC.Collect();
				GC.WaitForPendingFinalizers();
				await Task.Yield();

				Assert.NotNull(shapeView.Renderer);
				Assert.NotNull(shapeView.Drawable);
				Assert.NotNull(renderer.Drawable);

				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var graphicsView = renderer.GetType().GetField("_graphicsView", flags)?.GetValue(renderer) as PlatformGraphicsView;
				Assert.NotNull(graphicsView);
			});
		}
	}
}