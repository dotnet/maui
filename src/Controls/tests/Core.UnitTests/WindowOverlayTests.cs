using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using static Microsoft.Maui.Controls.Core.UnitTests.WindowsTests;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class WindowOverlayTests : BaseTestFixture
	{
		[Test]
		public void CreateAndRemoveOverlayWindow()
		{
			var app = new TestApp();
			var window = app.CreateWindow() as IWindow;
			app.LoadPage(new ContentPage());
			var windowOverlay = new WindowOverlay(window) as IWindowOverlay;

			// If not processed by a window, should be false.
			Assert.False(windowOverlay.IsPlatformViewInitialized);

			// First time inserted, should be true.
			Assert.True(window.AddOverlay(windowOverlay));

			// Should now be initialized.
			Assert.True(windowOverlay.IsPlatformViewInitialized);

			Assert.True(window.Overlays.Count > 0);

			// Can't insert same window overlay again, should be false.
			Assert.False(window.AddOverlay(windowOverlay));

			// Should remove from collection, should be true.
			Assert.True(window.RemoveOverlay(windowOverlay));

			// Not in collection, should be false.
			Assert.False(window.RemoveOverlay(windowOverlay));

			// Window was uninitialized, should be false.
			Assert.False(windowOverlay.IsPlatformViewInitialized);

			// Second time inserted, should be true.
			Assert.True(window.AddOverlay(windowOverlay));

			// Should now be initialized again.
			Assert.True(windowOverlay.IsPlatformViewInitialized);
		}

		[Test]
		public void CreateWindowOverlayAndElements()
		{
			var app = new TestApp();
			var window = app.CreateWindow() as IWindow;
			app.LoadPage(new ContentPage());
			var windowOverlay = new WindowOverlay(window) as IWindowOverlay;

			// First time inserted, should be true.
			Assert.True(window.AddOverlay(windowOverlay));

			var element = new TestWindowElement();

			// Adding element for the first time, should be true.
			Assert.True(windowOverlay.AddWindowElement(element));
			Assert.True(windowOverlay.WindowElements.Count > 0);

			// Can't add same element again, should be false.
			Assert.False(windowOverlay.AddWindowElement(element));

			// First time removing element, should be true.
			Assert.True(windowOverlay.RemoveWindowElement(element));

			Assert.True(windowOverlay.WindowElements.Count == 0);
		}

	}

	public class TestWindowElement : IWindowOverlayElement
	{
		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
		}

		public bool Contains(Point point)
		{
			return false;
		}
	}
}
