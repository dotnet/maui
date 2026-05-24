using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;


public class Icons
{
	public const string CLOSE = "ic_close.png";
}

public class MockxStatic
{
	public static string MockStaticProperty { get { return "Property"; } }
	public const string MockConstant = "Constant";
	public static string MockField = "Field";
	public static string MockFieldRef = Icons.CLOSE;
	public string InstanceProperty { get { return "InstanceProperty"; } }
	public static readonly Color BackgroundColor = Colors.Fuchsia;

	public class Nested
	{
		public static string Foo = "FOO";
	}
}

public enum MockEnum : long
{
	First,
	Second,
	Third,
}

public partial class XStatic : ContentPage
{
	public XStatic() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		//{x:Static Member=prefix:typeName.staticMemberName}
		//{x:Static prefix:typeName.staticMemberName}

		//The code entity that is referenced must be one of the following:
		// - A constant
		// - A static property
		// - A field
		// - An enumeration value
		// All other cases should throw

		[Theory]
		[XamlInflatorData]
		internal void StaticProperty(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("Property", layout.staticproperty.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void MemberOptional(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("Property", layout.memberisoptional.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void FieldColor(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal(Colors.Fuchsia, layout.color.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void Constant(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("Constant", layout.constant.Text);
		}

		//https://bugzilla.xamarin.com/show_bug.cgi?id=49228
		[Theory]
		[XamlInflatorData]
		internal void ConstantInARemoteAssembly(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("CompatibilityGalleryControls", layout.remoteConstant.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void Field(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("Field", layout.field.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void Enum(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal(ScrollOrientation.Both, layout.enuM.Orientation);
		}

		[Theory]
		[XamlInflatorData]
		internal void FieldRef(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("ic_close.png", layout.field2.Text);
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=48242
		[Theory]
		[XamlInflatorData]
		internal void xStaticAndImplicitOperators(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal("ic_close.png", (layout.ToolbarItems[0].IconImageSource as FileImageSource).File);
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=55096
		[Theory]
		[XamlInflatorData]
		internal void xStaticAndNestedClasses(XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.Equal(MockxStatic.Nested.Foo, layout.nestedField.Text);
		}
	}
}