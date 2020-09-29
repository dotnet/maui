using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
		public static readonly Color BackgroundColor = Color.Fuchsia;

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

		[TestFixture]
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

			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void StaticProperty(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("Property", layout.staticproperty.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void MemberOptional(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("Property", layout.memberisoptional.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void FieldColor(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual(Color.Fuchsia, layout.color.TextColor);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void Constant(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("Constant", layout.constant.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			//https://bugzilla.xamarin.com/show_bug.cgi?id=49228
			public void ConstantInARemoteAssembly(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("XamarinFormsControls", layout.remoteConstant.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void Field(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("Field", layout.field.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void Enum(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual(ScrollOrientation.Both, layout.enuM.Orientation);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void FieldRef(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("ic_close.png", layout.field2.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			// https://bugzilla.xamarin.com/show_bug.cgi?id=48242
			public void xStaticAndImplicitOperators(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual("ic_close.png", (layout.ToolbarItems[0].IconImageSource as FileImageSource).File);
			}

			[TestCase(false)]
			[TestCase(true)]
			// https://bugzilla.xamarin.com/show_bug.cgi?id=55096
			public void xStaticAndNestedClasses(bool useCompiledXaml)
			{
				var layout = new XStatic(useCompiledXaml);
				Assert.AreEqual(MockxStatic.Nested.Foo, layout.nestedField.Text);
			}
		}
	}
}