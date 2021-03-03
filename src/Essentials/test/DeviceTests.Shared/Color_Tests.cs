using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Color_Tests
	{
		byte a = 187;
		byte r = 52;
		byte g = 152;
		byte b = 219;

		[Fact]
		public void SystemToPlatform()
		{
			var system = System.Drawing.Color.FromArgb(a, r, g, b);
			var platform = system.ToPlatformColor();

#if __IOS__
            platform.GetRGBA(out var red, out var green, out var blue, out var alpha);
            Assert.Equal(a, (byte)(alpha * 255));
            Assert.Equal(r, (byte)(red * 255));
            Assert.Equal(g, (byte)(green * 255));
            Assert.Equal(b, (byte)(blue * 255));
#else
			Assert.Equal(a, platform.A);
			Assert.Equal(r, platform.R);
			Assert.Equal(g, platform.G);
			Assert.Equal(b, platform.B);
#endif
		}

		[Fact]
		public void PlatformToColor()
		{
#if __IOS__
            var platform = UIKit.UIColor.FromRGBA(r, g, b, a);
#elif __ANDROID__
            var platform = new Android.Graphics.Color(r, g, b, a);
#else
			var platform = Windows.UI.Color.FromArgb(a, r, g, b);
#endif

			var system = platform.ToSystemColor();
			Assert.Equal(a, system.A);
			Assert.Equal(r, system.R);
			Assert.Equal(g, system.G);
			Assert.Equal(b, system.B);
		}
	}
}
