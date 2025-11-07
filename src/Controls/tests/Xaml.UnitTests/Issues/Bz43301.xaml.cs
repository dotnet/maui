using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Foo.Microsoft.Maui.Controls.Bar;

public partial class Bz43301 : ContentPage
{
	public Bz43301() => InitializeComponent();


	public class Tests
	{
		[Fact]
		//No need for any actual [Theory]. If this compiles, the bug is fixed.
		public void DoesCompile(XamlInflator inflator)
		{
			var layout = new Bz43301(inflator);
			// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
		}
	}
}