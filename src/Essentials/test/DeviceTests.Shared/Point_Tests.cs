using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Point_Tests
	{
		int x = 50;
		int y = 40;

		[Fact]
		public void SystemToPlatform()
		{
			var system = new System.Drawing.Point(x, y);
			var platform = system.ToPlatformPoint();
			Assert.Equal(x, platform.X);
			Assert.Equal(y, platform.Y);
		}

		[Fact]
		public void PlatformToSystem()
		{
#if __IOS__
			var platform = new CoreGraphics.CGPoint(x, y);
#elif __ANDROID__
			var platform = new Android.Graphics.Point(x, y);
#else
			var platform = new Windows.Foundation.Point(x, y);
#endif
			var system = platform.ToSystemPoint();

			Assert.Equal(x, system.X);
			Assert.Equal(y, system.Y);
		}

		[Theory]
		[InlineData(float.MaxValue, 0)]
		[InlineData(0, float.MaxValue)]
		public void PlatformToSystemException(float x, float y)
		{
#if __IOS__
			var platform = new CoreGraphics.CGPoint(x, y);
			Assert.Throws<ArgumentOutOfRangeException>(() => platform.ToSystemPoint());
#elif __ANDROID__
			// N/A
			Utils.Unused(x, y);
#elif WINDOWS_UWP
			var platform = new Windows.Foundation.Point(x, y);
			Assert.Throws<ArgumentOutOfRangeException>(() => platform.ToSystemPoint());
#endif
		}

		[Fact]
		public void SystemToPlatformF()
		{
			var system = new System.Drawing.PointF(x, y);
#if __IOS__
			var platform = system.ToPlatformPoint();
#elif __ANDROID__
			var platform = system.ToPlatformPointF();
#else
			var platform = system.ToPlatformPoint();
#endif
			Assert.Equal(x, platform.X);
			Assert.Equal(y, platform.Y);
		}

		[Fact]
		public void PlatformToSystemF()
		{
#if __IOS__
			var platform = new CoreGraphics.CGPoint(x, y);
#elif __ANDROID__
			var platform = new Android.Graphics.PointF(x, y);
#else
			var platform = new Windows.Foundation.Point(x, y);
#endif
			var system = platform.ToSystemPointF();
			Assert.Equal(x, system.X);
			Assert.Equal(y, system.Y);
		}
	}
}
