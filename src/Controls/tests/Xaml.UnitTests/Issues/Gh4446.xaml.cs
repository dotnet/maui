using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh4446Item
{
	public string Id { get; set; }
	public string Text { get; set; }
	public string Description { get; set; }
}

public partial class Gh4446 : ContentPage
{
	public Gh4446() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void BindingThrowsOnWrongConverterParameter(XamlInflator inflator)
		{
			var page = new Gh4446(inflator) { BindingContext = new Gh4446Item { Text = null } };
			Assert.NotNull(page);
		}
	}
}
