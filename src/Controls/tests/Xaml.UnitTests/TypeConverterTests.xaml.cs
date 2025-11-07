using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeConverterTests : ContentPage
{
	public TypeConverterTests() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void UriAreConverted(XamlInflator inflator)
		{
			var layout = new TypeConverterTests(inflator);
			Assert.IsType<Uri>(layout.imageSource.Uri);
			Assert.Equal("https://xamarin.com/content/images/pages/branding/assets/xamagon.png", layout.imageSource.Uri.ToString());
		}
	}
}