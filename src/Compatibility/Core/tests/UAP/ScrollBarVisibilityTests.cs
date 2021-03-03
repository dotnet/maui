using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class ScrollBarVisibilityTests
	{
		[Test, Category("Scrollbar")]
		public void ConvertScrollbarVisibility()
		{
			var always = ScrollBarVisibility.Always.ToUwpScrollBarVisibility();
			var defaultVisibility = ScrollBarVisibility.Default.ToUwpScrollBarVisibility();
			var never = ScrollBarVisibility.Never.ToUwpScrollBarVisibility();

			Assert.That(always, Is.EqualTo(Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Visible));
			Assert.That(defaultVisibility, Is.EqualTo(Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Auto));
			Assert.That(never, Is.EqualTo(Microsoft.UI.Xaml.Controls.ScrollBarVisibility.Hidden));
		}
	}
}
