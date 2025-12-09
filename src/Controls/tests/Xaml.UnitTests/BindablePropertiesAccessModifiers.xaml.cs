using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

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

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = new MockApplication();
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void BindProperties(XamlInflator inflator)
		{
			var page = new BindablePropertiesAccessModifiers(inflator) { BindingContext = new BindablePropertiesAccessModifiersVM() };
			Assert.Equal("Bar", page.AMC.GetValue(AccessModifiersControl.InternalBarProperty));
			Assert.Equal("Foo", page.AMC.GetValue(AccessModifiersControl.PublicFooProperty));
		}
	}
}
