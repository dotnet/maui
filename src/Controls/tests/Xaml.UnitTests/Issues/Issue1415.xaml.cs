using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue1415 : ContentPage
{
	public Issue1415() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void NestedMarkupExtension(XamlInflator inflator)
		{
			var page = new Issue1415(inflator);
			var label = page.FindByName<Label>("label");
			Assert.NotNull(label);
			label.BindingContext = "foo";
			Assert.Equal("oof", label.Text);
		}
	}
}