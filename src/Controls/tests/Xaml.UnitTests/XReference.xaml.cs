using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XReference : ContentPage
{
	public XReference() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void SupportsXReference(XamlInflator inflator)
		{
			var layout = new XReference(inflator);
			Assert.Same(layout.image, layout.imageView.Content);
		}

		[Theory]
		[Values]
		public void XReferenceAsCommandParameterToSelf(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			var button = layout.aButton;
			button.BindingContext = new
			{
				ButtonClickCommand = new Command(o =>
				{
					if (o == button)
					{
						// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
					}
				})
			};
			((IButtonController)button).SendClicked();
			Assert.Fail();
		}

		[Theory]
		[Values]
		public void XReferenceAsBindingSource(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.Equal("foo", layout.entry.Text);
			Assert.Equal("bar", layout.entry.Placeholder);
		}

		[Theory]
		[Values]
		public void CrossXReference(XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.Same(layout.label0, layout.label1.BindingContext);
			Assert.Same(layout.label1, layout.label0.BindingContext);
		}
	}
}
