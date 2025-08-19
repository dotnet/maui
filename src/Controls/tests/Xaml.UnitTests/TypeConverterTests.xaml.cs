using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class TypeConverterTests : ContentPage
{
	public TypeConverterTests() => InitializeComponent();

	public class Tests
	{
		[Test]
		public void GridLengthsAreConverted([Values] XamlInflator inflator,
			[Values("en-US", "pt-PT")] string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.CreateMauiCompilation()
						.WithAdditionalSource(
							"""
							using System;
							using NUnit.Framework;

							namespace Microsoft.Maui.Controls.Xaml.UnitTests;

							[XamlProcessing(XamlInflator.Default, true)]
							public partial class TypeConverterTests : ContentPage
							{
								public TypeConverterTests() => InitializeComponent();
							}
							""").RunMauiSourceGenerator(typeof(TypeConverterTests));

				Assert.That(result.Diagnostics, Is.Empty);
			}
			var page = new TypeConverterTests(inflator);
			Assert.That(page.grid, Is.TypeOf<Grid>());
			Assert.AreEqual(4, page.grid.RowDefinitions.Count);
			Assert.AreEqual(GridLength.Star, page.grid.RowDefinitions[0].Height);
			Assert.AreEqual(10, page.grid.RowDefinitions[1].Height.Value);
			Assert.AreEqual(GridLength.Auto, page.grid.RowDefinitions[2].Height);
			Assert.AreEqual(1.25, page.grid.RowDefinitions[3].Height.Value);
		}

		[Test]
		public void UriAreConverted([Values] XamlInflator inflator)
		{
			var layout = new TypeConverterTests(inflator);
			Assert.That(layout.imageSource.Uri, Is.TypeOf<Uri>());
			Assert.AreEqual("https://xamarin.com/content/images/pages/branding/assets/xamagon.png", layout.imageSource.Uri.ToString());
		}
	}
}