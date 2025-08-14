using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using NUnit.Framework;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class PlatformSpecific : FlyoutPage
{
	public PlatformSpecific() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void PlatformSpecificPropertyIsSet([Values] XamlInflator inflator)
		{
			var layout = new PlatformSpecific(inflator);
			Assert.AreEqual(layout.On<WindowsOS>().GetCollapseStyle(), CollapseStyle.Partial);
			Assert.AreEqual(layout.On<WindowsOS>().CollapsedPaneWidth(), 96d);
		}
	}
}