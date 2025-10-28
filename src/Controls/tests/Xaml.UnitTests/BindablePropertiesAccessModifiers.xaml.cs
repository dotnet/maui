using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class AccessModifiersControl : View
	{
		public static BindableProperty PublicFooProperty = BindableProperty.Create("PublicFoo",
			typeof(string),
			typeof(AccessModifiersControl),
			"");

		public string PublicFoo
		{
			get => (string)GetValue(PublicFooProperty);
			set => SetValue(PublicFooProperty, value);
		}

		internal static BindableProperty InternalBarProperty = BindableProperty.Create("InternalBar",
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


		public BindablePropertiesAccessModifiers()
		{
			InitializeComponent();
		}

		public BindablePropertiesAccessModifiers(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void BindProperties(bool useCompiledXaml)
			{
				var page = new BindablePropertiesAccessModifiers(useCompiledXaml);
				page.BindingContext = new BindablePropertiesAccessModifiersVM();
				Assert.Equal("Bar", page.AMC.GetValue(AccessModifiersControl.InternalBarProperty));
				Assert.Equal("Foo", page.AMC.GetValue(AccessModifiersControl.PublicFooProperty));
			}
		}
	}
}