using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeConverterTests : ContentPage
{
	public TypeConverterTests() => InitializeComponent();

	class Tests
	{
		[Test]
		public void UriAreConverted([Values] XamlInflator inflator)
		{
			var layout = new TypeConverterTests(inflator);
			Assert.That(layout.imageSource.Uri, Is.TypeOf<Uri>());
			Assert.AreEqual("https://xamarin.com/content/images/pages/branding/assets/xamagon.png", layout.imageSource.Uri.ToString());
		}
	}
}