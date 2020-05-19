using NUnit.Framework;
using System.Maui.Platform.UWP;

namespace System.Maui.Platform.UAP.UnitTests
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

			Assert.That(always, Is.EqualTo(global::Windows.UI.Xaml.Controls.ScrollBarVisibility.Visible));
			Assert.That(defaultVisibility, Is.EqualTo(global::Windows.UI.Xaml.Controls.ScrollBarVisibility.Auto));
			Assert.That(never, Is.EqualTo(global::Windows.UI.Xaml.Controls.ScrollBarVisibility.Hidden));
		}
	}
}
