using NUnit.Framework;
using System.Maui.Platform.UWP;

namespace System.Maui.Platform.UAP.UnitTests
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
