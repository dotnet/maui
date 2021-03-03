using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class ColorTests
	{
		[Test, Category("Color")]
		public void PrimaryColorConversions() 
		{
			// 11:15, restate my assumptions...

			var windowsRed = Color.Red.ToWindowsColor();
			Assert.That(windowsRed.R, Is.EqualTo(255));

			var windowsBlue = Color.Blue.ToWindowsColor();
			Assert.That(windowsBlue.B, Is.EqualTo(255));

			var windowsGreen = Color.Green.ToWindowsColor();
			Assert.That(windowsGreen.G, Is.EqualTo(128));
		}
	}
}
