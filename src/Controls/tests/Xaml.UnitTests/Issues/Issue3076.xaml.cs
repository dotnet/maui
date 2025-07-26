using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public class Issue3076Button : Button
{
	public static readonly BindableProperty VerticalContentAlignmentProperty =
		BindableProperty.Create(nameof(VerticalContentAlignment), typeof(TextAlignment), typeof(Issue3076Button), TextAlignment.Center);

	public TextAlignment VerticalContentAlignment
	{
		get { return (TextAlignment)GetValue(VerticalContentAlignmentProperty); }
		set { SetValue(VerticalContentAlignmentProperty, value); }
	}
}

[XamlProcessing(XamlInflator.Default, true)]
public partial class Issue3076 : ContentPage
{
	public Issue3076() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void CanUseBindableObjectDefinedInThisAssembly([Values] XamlInflator inflator)
		{
			var layout = new Issue3076(inflator);

			Assert.That(layout.local, Is.TypeOf<Issue3076Button>());
			Assert.AreEqual(TextAlignment.Start, layout.local.VerticalContentAlignment);
		}

		[Test]
		public void CanUseBindableObjectDefinedInOtherAssembly([Values] XamlInflator inflator)
		{
			var layout = new Issue3076(inflator);

			Assert.That(layout.controls, Is.TypeOf<Microsoft.Maui.Controls.ControlGallery.Issue3076Button>());
			Assert.AreEqual(TextAlignment.Start, layout.controls.HorizontalContentAlignment);
		}
	}
}