using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using NUnit.Framework;

namespace Foo.Microsoft.Maui.Controls.Bar;

public partial class Bz43301 : ContentPage
{
	public Bz43301() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		//No need for any actual [Test]. If this compiles, the bug is fixed.
		public void DoesCompile([Values] XamlInflator inflator)
		{
			var layout = new Bz43301(inflator);
			Assert.Pass();
		}
	}
}