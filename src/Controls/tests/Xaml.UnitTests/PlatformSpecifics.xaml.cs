using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Xunit;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class PlatformSpecific : FlyoutPage
{
	public PlatformSpecific() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void PlatformSpecificPropertyIsSet(XamlInflator inflator)
		{
			var layout = new PlatformSpecific(inflator);
			Assert.Equal(CollapseStyle.Partial, layout.On<WindowsOS>().GetCollapseStyle());
			Assert.Equal(96d, layout.On<WindowsOS>().CollapsedPaneWidth());
		}
	}
}