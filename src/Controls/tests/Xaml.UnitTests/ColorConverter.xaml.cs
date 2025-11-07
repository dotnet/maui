using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class ColorConverterVM
{
	public string ButtonBackground => "#fc87ad";
}

public partial class ColorConverter : ContentPage
{

	public ColorConverter() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void StringsAreValidAsColor(XamlInflator inflator)
		{
			var page = new ColorConverter(inflator);
			page.BindingContext = new ColorConverterVM();

			var expected = Color.FromArgb("#fc87ad");
			Assert.Equal(expected, page.Button0.BackgroundColor);
		}
	}
}
