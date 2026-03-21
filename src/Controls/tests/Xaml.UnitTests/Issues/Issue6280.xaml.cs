using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue6280 : ContentPage
{
	public Issue6280() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindingToNullable(XamlInflator inflator)
		{
			var vm = new Issue6280ViewModel();
			var page = new Issue6280(inflator) { BindingContext = vm };
			page._entry.SetValueFromRenderer(Entry.TextProperty, 1);
			Assert.Equal(1, vm.NullableInt);
		}
	}
}

public class Issue6280ViewModel
{
	public int? NullableInt { get; set; }
}