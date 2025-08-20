using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class AccessModifiersControl : View
{
	public static BindableProperty PublicFooProperty = BindableProperty.Create(nameof(PublicFoo),
		typeof(string),
		typeof(AccessModifiersControl),
		"");

	public string PublicFoo
	{
		get => (string)GetValue(PublicFooProperty);
		set => SetValue(PublicFooProperty, value);
	}

	internal static BindableProperty InternalBarProperty = BindableProperty.Create(nameof(InternalBar),
		typeof(string),
		typeof(AccessModifiersControl),
		"");

	public string InternalBar
	{
		get => (string)GetValue(InternalBarProperty);
		set => SetValue(InternalBarProperty, value);
	}
}

public class BindablePropertiesAccessModifiersVM
{
	public string Foo => "Foo";
	public string Bar => "Bar";
}

public partial class BindablePropertiesAccessModifiers : ContentPage
{

	public BindablePropertiesAccessModifiers() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => Application.Current = new MockApplication();
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void BindProperties([Values] XamlInflator inflator)
		{
			var page = new BindablePropertiesAccessModifiers(inflator) { BindingContext = new BindablePropertiesAccessModifiersVM() };
			Assert.AreEqual("Bar", page.AMC.GetValue(AccessModifiersControl.InternalBarProperty));
			Assert.AreEqual("Foo", page.AMC.GetValue(AccessModifiersControl.PublicFooProperty));
		}
	}
}
