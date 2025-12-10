using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported006 : ContentPage
{
	public Unreported006() => InitializeComponent();

	public Compatibility.Layout<View> GenericProperty
	{
		get { return (Compatibility.Layout<View>)GetValue(GenericPropertyProperty); }
		set { SetValue(GenericPropertyProperty, value); }
	}

	public static readonly BindableProperty GenericPropertyProperty =
		BindableProperty.Create(nameof(GenericProperty), typeof(Controls.Compatibility.Layout<View>), typeof(Unreported006));

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CanAssignGenericBP(XamlInflator inflator)
		{
			var page = new Unreported006(inflator);
			Assert.NotNull(page.GenericProperty);
			Assert.IsType<Compatibility.StackLayout>(page.GenericProperty);
		}
	}
}