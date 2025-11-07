using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DynamicResource : ContentPage
{
	public DynamicResource()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void TestDynamicResources(XamlInflator inflator)
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