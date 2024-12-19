using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class DynamicResource : ContentPage
{
	public DynamicResource()
	{
		InitializeComponent();
	}

	[TestFixture]
	public class Tests
	{
		[Test]
		public void TestDynamicResources([Values]XamlInflator inflator)
		{
			var layout = new DynamicResource(inflator);
			var label = layout.label0;

			Assert.Null(label.Text);

			layout.Resources = new ResourceDictionary {
				{"FooBar", "FOOBAR"},
			};
			Assert.AreEqual("FOOBAR", label.Text);
		}
	}
}