using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "Microsoft.Maui.Controls.Xaml.UnitTests.NSGlobalXmlnsWithXStatic")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests.NSGlobalXmlnsWithXStatic
{
	public class MockxStatic
	{
		public static string MockStaticProperty { get { return "Property"; } }
		public const string MockConstant = "MConstant";
		public static string MockField = "Field";
		public static string MockFieldRef = Icons.CLOSE;
		public string InstanceProperty { get { return "InstanceProperty"; } }
		public static readonly Color BackgroundColor = Colors.Fuchsia;

		public class Nested
		{
			public static string Foo = "FOO";
		}
	}
}

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class GlobalXmlnsWithXStatic : ContentPage
	{
		public GlobalXmlnsWithXStatic()
		{
			InitializeComponent();
		}

		public GlobalXmlnsWithXStatic(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[Test]
		public void XStaticWithAggregatedXmlns([Values] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(GlobalXmlnsWithXStatic));

			var page = new GlobalXmlnsWithXStatic(useCompiledXaml);
			Assert.That(page.label0.Text, Is.EqualTo("MConstant"));
		}


	}
}