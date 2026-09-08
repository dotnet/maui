using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DynamicResource : ContentPage
{
	public DynamicResource()
	{
		InitializeComponent();
	}

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void TestDynamicResources(XamlInflator inflator)
		{
			var layout = new DynamicResource(inflator);
			var label = layout.label0;

			Assert.Null(label.Text);

			layout.Resources = new ResourceDictionary {
				{"FooBar", "FOOBAR"},
			};
			Assert.Equal("FOOBAR", label.Text);
		}
	}
}