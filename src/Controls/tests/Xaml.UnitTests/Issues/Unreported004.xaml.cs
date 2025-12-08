using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported004 : ContentPage
{
	public Unreported004() => InitializeComponent();

	public static readonly BindableProperty SomePropertyProperty =
		BindableProperty.Create("SomeProperty", typeof(string),
		typeof(Unreported004), null);

	public static string GetSomeProperty(BindableObject bindable) => bindable.GetValue(SomePropertyProperty) as string;
	public static string GetSomeProperty(BindableObject bindable, object foo) => null;
	public static void SetSomeProperty(BindableObject bindable, string value) => bindable.SetValue(SomePropertyProperty, value);

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void MultipleGetMethodsAllowed(XamlInflator inflator)
		{
			var page = new Unreported004(inflator);
			Assert.NotNull(page.label);
			Assert.Equal("foo", GetSomeProperty(page.label));
		}
	}
}

