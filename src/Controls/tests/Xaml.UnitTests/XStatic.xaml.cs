using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

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
		public XStatic()
		{
			InitializeComponent();
		}
		public XStatic(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			//{x:Static Member=prefix:typeName.staticMemberName}
			//{x:Static prefix:typeName.staticMemberName}

			//The code entity that is referenced must be one of the following:
			// - A constant
			// - A static property
			// - A field
			// - An enumeration value
			// All other cases should throw

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void StaticProperty(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("Property", layout.staticproperty.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void MemberOptional(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("Property", layout.memberisoptional.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void FieldColor(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal(Colors.Fuchsia, layout.color.TextColor);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void Constant(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("Constant", layout.constant.Text);
			}

			[Xunit.Theory]
			[InlineData(false)]
			[InlineData(true)]
			//https://bugzilla.xamarin.com/show_bug.cgi?id=49228
			public void ConstantInARemoteAssembly(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("CompatibilityGalleryControls", layout.remoteConstant.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void Field(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("Field", layout.field.Text);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void Enum(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal(ScrollOrientation.Both, layout.enuM.Orientation);
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void FieldRef(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("ic_close.png", layout.field2.Text);
			}

			[Xunit.Theory]
			[InlineData(false)]
			[InlineData(true)]
			// https://bugzilla.xamarin.com/show_bug.cgi?id=48242
			public void xStaticAndImplicitOperators(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal("ic_close.png", (layout.ToolbarItems[0].IconImageSource as FileImageSource).File);
			}

			[Xunit.Theory]
			[InlineData(false)]
			[InlineData(true)]
			// https://bugzilla.xamarin.com/show_bug.cgi?id=55096
			public void xStaticAndNestedClasses(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.Equal(MockxStatic.Nested.Foo, layout.nestedField.Text);
			}
		}
	}
}