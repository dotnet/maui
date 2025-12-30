using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Simple item class for the test
public class Maui24472Item
{
	public string Name { get; set; }
}

public partial class Maui24472 : ContentPage
{
	public Maui24472() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		// https://github.com/dotnet/maui/issues/24472
		// XamlC was throwing "Value cannot be null. (Parameter 'key')" when compiling
		// XAML with x:Array containing custom objects in CollectionView.ItemsSource
		internal void XArrayInCollectionViewItemsSourceDoesNotCrash(XamlInflator inflator)
		{
			var page = new Maui24472(inflator);
			Assert.NotNull(page);
		}
	}
}
