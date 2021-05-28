using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class ColorTests
	{
		[Test, Category("Color")]
		public void PrimaryColorConversions()
		{
			// 11:15, restate my assumptions...

			var windowsRed = Colors.Red.ToWindowsColor();
			Assert.That(windowsRed.R, Is.EqualTo(255));

			var windowsBlue = Colors.Blue.ToWindowsColor();
			Assert.That(windowsBlue.B, Is.EqualTo(255));

			var windowsGreen = Colors.Green.ToWindowsColor();
			Assert.That(windowsGreen.G, Is.EqualTo(128));
		}
	}
}
