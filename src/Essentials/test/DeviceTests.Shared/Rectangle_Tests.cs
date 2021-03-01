using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Rectangle_Tests
	{
		int x = 50;
		int y = 40;
		int width = 100;
		int height = 80;

		[Fact]
		public void SystemToPlatform()
		{
			var system = new System.Drawing.Rectangle(x, y, width, height);
			var platform = system.ToPlatformRectangle();
#if __ANDROID__
			Assert.Equal(x, platform.Left);
			Assert.Equal(y, platform.Top);
			Assert.Equal(system.Left, platform.Left);
			Assert.Equal(system.Top, platform.Top);
			Assert.Equal(width, platform.Width());
			Assert.Equal(height, platform.Height());
#else
			Assert.Equal(x, platform.X);
			Assert.Equal(y, platform.Y);
			Assert.Equal(width, platform.Width);
			Assert.Equal(height, platform.Height);
#endif
		}

		[Fact]
		public void PlatformToSystem()
		{
#if __IOS__
			var platform = new CoreGraphics.CGRect(x, y, width, height);
#elif __ANDROID__
			var platform = new Android.Graphics.Rect(x, y, x + width, y + height);
#else
			var platform = new Windows.Foundation.Rect(x, y, width, height);
#endif
			var system = platform.ToSystemRectangle();

			Assert.Equal(x, system.X);
			Assert.Equal(y, system.Y);
			Assert.Equal(width, system.Width);
			Assert.Equal(height, system.Height);
		}

		[Theory]
		[InlineData(float.MaxValue, 0, 0, 0)]
		[InlineData(0, float.MaxValue, 0, 0)]
		[InlineData(0, 0, float.MaxValue, 0)]
		[InlineData(0, 0, 0, float.MaxValue)]
		public void PlatformToSystemException(float x, float y, float width, float height)
		{
#if __IOS__
			var platform = new CoreGraphics.CGRect(x, y, width, height);
			Assert.Throws<ArgumentOutOfRangeException>(() => platform.ToSystemRectangle());
#elif __ANDROID__
			// N/A
			Utils.Unused(x, y, width, height);
#elif WINDOWS_UWP
			var platform = new Windows.Foundation.Rect(x, y, width, height);
			Assert.Throws<ArgumentOutOfRangeException>(() => platform.ToSystemRectangle());
#endif
		}

		[Fact]
		public void SystemToPlatformF()
		{
			var system = new System.Drawing.RectangleF(x, y, width, height);
#if __ANDROID__
			var platform = system.ToPlatformRectangleF();
			Assert.Equal(x, platform.Left);
			Assert.Equal(y, platform.Top);
			Assert.Equal(width, platform.Width());
			Assert.Equal(height, platform.Height());
#else
			var platform = system.ToPlatformRectangle();
			Assert.Equal(x, platform.X);
			Assert.Equal(y, platform.Y);
			Assert.Equal(width, platform.Width);
			Assert.Equal(height, platform.Height);
#endif
		}

		[Fact]
		public void PlatformToSystemF()
		{
#if __IOS__
			var platform = new CoreGraphics.CGRect(x, y, width, height);
#elif __ANDROID__
			var platform = new Android.Graphics.RectF(x, y, x + width, y + height);
#else
			var platform = new Windows.Foundation.Rect(x, y, width, height);
#endif

			var system = platform.ToSystemRectangleF();
			Assert.Equal(x, system.X);
			Assert.Equal(y, system.Y);
			Assert.Equal(width, system.Width);
			Assert.Equal(height, system.Height);
		}
	}
}
