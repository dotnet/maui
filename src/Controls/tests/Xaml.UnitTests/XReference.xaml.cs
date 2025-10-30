using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XReference : ContentPage
{
	public XReference() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void SupportsXReference([Values] XamlInflator inflator)
		{
			var layout = new XReference(inflator);
			Assert.AreSame(layout.image, layout.imageView.Content);
		}

		[Test]
		public void XReferenceAsCommandParameterToSelf([Values] XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			var button = layout.aButton;
			button.BindingContext = new
			{
				ButtonClickCommand = new Command(o =>
				{
					if (o == button)
						Assert.Pass();
				})
			};
			((IButtonController)button).SendClicked();
			Assert.Fail();
		}

		[Test]
		public void XReferenceAsBindingSource([Values] XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.AreEqual("foo", layout.entry.Text);
			Assert.AreEqual("bar", layout.entry.Placeholder);
		}

		[Test]
		public void CrossXReference([Values] XamlInflator inflator)
		{
			var layout = new XReference(inflator);

			Assert.AreSame(layout.label0, layout.label1.BindingContext);
			Assert.AreSame(layout.label1, layout.label0.BindingContext);
		}
	}
}