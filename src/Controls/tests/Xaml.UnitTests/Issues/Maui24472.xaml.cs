using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Simple item class for the test
public class Maui24472Item
{
	public string Name { get; set; }
}

public partial class Maui24472 : ContentPage
{
	public Maui24472() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		// https://github.com/dotnet/maui/issues/24472
		// XamlC was throwing "Value cannot be null. (Parameter 'key')" when compiling
		// XAML with x:Array containing custom objects in CollectionView.ItemsSource
		public void XArrayInCollectionViewItemsSourceDoesNotCrash([Values] XamlInflator inflator)
		{
			var page = new Maui24472(inflator);
			Assert.That(page, Is.Not.Null);
		}
	}
}
