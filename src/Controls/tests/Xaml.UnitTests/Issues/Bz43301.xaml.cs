using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.UnitTests;
using Xunit;

namespace Foo.Microsoft.Maui.Controls.Bar;

public partial class Bz43301 : ContentPage
{
	public Bz43301() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		//If this compiles, the bug is fixed.
		public void DoesCompile(XamlInflator inflator)
		{
			var layout = new Bz43301(inflator);
			// test passes if no exception is thrown
		}
	}
}