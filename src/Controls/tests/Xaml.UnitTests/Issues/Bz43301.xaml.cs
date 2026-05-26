using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.UnitTests;
using Xunit;

namespace Foo.Microsoft.Maui.Controls.Bar;

public partial class Bz43301 : ContentPage
{
	public Bz43301() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		//No need for any actual [Test]. If this compiles, the bug is fixed.
		internal void DoesCompile(XamlInflator inflator)
		{
			var layout = new Bz43301(inflator);
			// Test passes if no exception is thrown
		}
	}
}