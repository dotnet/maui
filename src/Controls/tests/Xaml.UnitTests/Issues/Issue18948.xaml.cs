using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue18948 : Shell
{
	public Issue18948() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void NavBarIsVisiblePropertyPropagates(XamlInflator inflator)
		{
			var shell = new Issue18948(inflator);
			var navBarIsVisible = Shell.GetNavBarIsVisible(shell.CurrentContent);
			Assert.False(navBarIsVisible);
		}
	}
}

