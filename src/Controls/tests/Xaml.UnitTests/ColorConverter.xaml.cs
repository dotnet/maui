using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class ColorConverterVM
{
	public string ButtonBackground => "#fc87ad";
}

public partial class ColorConverter : ContentPage
{

	public ColorConverter() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void StringsAreValidAsColor([Values] XamlInflator inflator)
		{
			var page = new ColorConverter(inflator);
			page.BindingContext = new ColorConverterVM();

			var expected = Color.FromArgb("#fc87ad");
			Assert.AreEqual(expected, page.Button0.BackgroundColor);
		}
	}
}
