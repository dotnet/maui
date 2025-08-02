using Microsoft.Maui.Graphics;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		//{x:Static Member=prefix:typeName.staticMemberName}
		//{x:Static prefix:typeName.staticMemberName}

		//The code entity that is referenced must be one of the following:
		// - A constant
		// - A static property
		// - A field
		// - An enumeration value
		// All other cases should throw

		[Test]
		public void StaticProperty([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("Property", layout.staticproperty.Text);
		}

		[Test]
		public void MemberOptional([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("Property", layout.memberisoptional.Text);
		}

		[Test]
		public void FieldColor([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual(Colors.Fuchsia, layout.color.TextColor);
		}

		[Test]
		public void Constant([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("Constant", layout.constant.Text);
		}

		//https://bugzilla.xamarin.com/show_bug.cgi?id=49228
		[Test]
		public void ConstantInARemoteAssembly([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("CompatibilityGalleryControls", layout.remoteConstant.Text);
		}

		[Test]
		public void Field([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("Field", layout.field.Text);
		}

		[Test]
		public void Enum([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual(ScrollOrientation.Both, layout.enuM.Orientation);
		}

		[Test]
		public void FieldRef([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("ic_close.png", layout.field2.Text);
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=48242
		[Test]
		public void xStaticAndImplicitOperators([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual("ic_close.png", (layout.ToolbarItems[0].IconImageSource as FileImageSource).File);
		}

		// https://bugzilla.xamarin.com/show_bug.cgi?id=55096
		[Test]
		public void xStaticAndNestedClasses([Values] XamlInflator inflator)
		{
			var layout = new XStatic(inflator);
			Assert.AreEqual(MockxStatic.Nested.Foo, layout.nestedField.Text);
		}
	}
}