using NUnit.Framework;

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

	class Tests
	{
		[Test]
		public void MultipleGetMethodsAllowed([Values] XamlInflator inflator)
		{
			var page = new Unreported004(inflator);
			Assert.NotNull(page.label);
			Assert.AreEqual("foo", GetSomeProperty(page.label));
		}
	}
}

