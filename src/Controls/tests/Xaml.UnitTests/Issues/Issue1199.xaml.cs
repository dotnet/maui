using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1199 : ContentPage
{
	public Issue1199() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void AllowCreationOfTypesFromString([Values] XamlInflator inflator)
		{
			var layout = new Issue1199(inflator);
			var res = (Color)layout.Resources["AlmostSilver"];

			Assert.AreEqual(Color.FromArgb("#FFCCCCCC"), res);
		}
	}
}