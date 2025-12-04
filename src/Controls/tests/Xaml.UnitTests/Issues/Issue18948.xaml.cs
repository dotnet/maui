using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue18948 : Shell
{
	public Issue18948() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void NavBarIsVisiblePropertyPropagates([Values] XamlInflator inflator)
		{
			var shell = new Issue18948(inflator);
			var navBarIsVisible = Shell.GetNavBarIsVisible(shell.CurrentContent);
			Assert.False(navBarIsVisible);
		}
	}
}

