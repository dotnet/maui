using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class BaseClassForStaticTest
{
	public const string BaseClassString = "base class string";
	public static string BaseStaticProperty { get; } = "base static property";
	public static string BaseStaticField = "base static field";
}

public class DerivedClassForStaticTest : BaseClassForStaticTest
{
	public const string DerivedClassString = "derived class string";
}

public partial class XStaticInheritance : ContentPage
{
	public XStaticInheritance() => InitializeComponent();

	public XStaticInheritance(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void XStaticCanAccessInheritedConstant(XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.Equal("base class string", layout.baseConstant.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void XStaticCanAccessInheritedStaticProperty(XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.Equal("base static property", layout.baseProperty.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void XStaticCanAccessInheritedStaticField(XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.Equal("base static field", layout.baseField.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void XStaticCanAccessDerivedClassMember(XamlInflator inflator)
		{
			var layout = new XStaticInheritance(inflator);
			Assert.Equal("derived class string", layout.derivedConstant.Text);
		}
	}
}
