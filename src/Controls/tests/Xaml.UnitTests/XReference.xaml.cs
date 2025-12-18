using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XReference : ContentPage
{
	public XReference() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SupportsXReference(XamlInflator inflator)
		{
			var layout = new XReference(inflator);
			Assert.Same(layout.image, layout.imageView.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void XReferenceAsBindingSource(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.Equal("foo", layout.entry.Text);
			Assert.Equal("bar", layout.entry.Placeholder);
		}

		[Theory]
		[XamlInflatorData]
		internal void CrossXReference(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.Same(layout.label0, layout.label1.BindingContext);
			Assert.Same(layout.label1, layout.label0.BindingContext);
		}
	}
}