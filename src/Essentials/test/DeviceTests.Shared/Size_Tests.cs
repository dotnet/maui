using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Size_Tests
	{
		int width = 50;
		int height = 40;

		[Fact]
		public void SystemToPlatform()
		{
			var system = new System.Drawing.Size(width, height);
			var platform = system.ToPlatformSize();
			Assert.Equal(width, platform.Width);
			Assert.Equal(height, platform.Height);
		}

		[Fact]
		public void PlatformToSystem()
		{
#if __IOS__
			var platform = new CoreGraphics.CGSize(width, height);
#elif __ANDROID__
			var platform = new Android.Util.Size(width, height);
#else
			var platform = new Windows.Foundation.Size(width, height);
#endif
			var system = platform.ToSystemSize();

			Assert.Equal(width, system.Width);
			Assert.Equal(height, system.Height);
		}

		[Theory]
		[InlineData(float.MaxValue, 0)]
		[InlineData(0, float.MaxValue)]
		public void PlatformToSystemException(float width, float height)
		{
#if __IOS__
			var platform = new CoreGraphics.CGSize(width, height);
			Assert.Throws<ArgumentOutOfRangeException>(() => platform.ToSystemSize());
#elif __ANDROID__
			// N/A
			Utils.Unused(width, height);
#elif WINDOWS_UWP
			var platform = new Windows.Foundation.Size(width, height);
			Assert.Throws<ArgumentOutOfRangeException>(() => platform.ToSystemSize());
#endif
		}

		[Fact]
		public void SystemToPlatformF()
		{
			var system = new System.Drawing.SizeF(width, height);
#if __IOS__
			var platform = system.ToPlatformSize();
#elif __ANDROID__
			var platform = system.ToPlatformSizeF();
#else
			var platform = system.ToPlatformSize();
#endif
			Assert.Equal(width, platform.Width);
			Assert.Equal(height, platform.Height);
		}

		[Fact]
		public void PlatformToSystemF()
		{
#if __IOS__
			var platform = new CoreGraphics.CGSize(width, height);
			var system = platform.ToSystemSize();
#elif __ANDROID__
			var platform = new Android.Util.SizeF(width, height);
			var system = platform.ToSystemSizeF();
#else
			var platform = new Windows.Foundation.Size(width, height);
			var system = platform.ToSystemSize();
#endif

			Assert.Equal(width, system.Width);
			Assert.Equal(height, system.Height);
		}
	}
}
