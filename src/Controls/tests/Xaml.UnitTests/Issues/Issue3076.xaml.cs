using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

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

public partial class Issue3076 : ContentPage
{
	public Issue3076() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CanUseBindableObjectDefinedInThisAssembly(XamlInflator inflator)
		{
			var layout = new Issue3076(inflator);

			Assert.IsType<Issue3076Button>(layout.local);
			Assert.Equal(TextAlignment.Start, layout.local.VerticalContentAlignment);
		}

		[Theory]
		[XamlInflatorData]
		internal void CanUseBindableObjectDefinedInOtherAssembly(XamlInflator inflator)
		{
			var layout = new Issue3076(inflator);

			Assert.IsType<Microsoft.Maui.Controls.ControlGallery.Issue3076Button>(layout.controls);
			Assert.Equal(TextAlignment.Start, layout.controls.HorizontalContentAlignment);
		}
	}
}