using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue6280 : ContentPage
{
	public Issue6280() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingToNullable(XamlInflator inflator)
		{
			var vm = new Issue6280ViewModel();
			var page = new Issue6280(inflator) { BindingContext = vm };
			page._entry.SetValueFromRenderer(Entry.TextProperty, 1);
			Assert.Equal(vm.NullableInt, 1);
		}
	}
}

public class Issue6280ViewModel
{
	public int? NullableInt { get; set; }
}