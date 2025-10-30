using NUnit.Framework;

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

	class Tests
	{
		[Test]
		public void CanAssignGenericBP([Values] XamlInflator inflator)
		{
			var page = new Unreported006(inflator);
			Assert.NotNull(page.GenericProperty);
			Assert.That(page.GenericProperty, Is.TypeOf<Compatibility.StackLayout>());
		}
	}
}