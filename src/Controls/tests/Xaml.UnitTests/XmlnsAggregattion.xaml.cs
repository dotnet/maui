using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/maui/global", "http://schemas.microsoft.com/dotnet/2021/maui")]

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XmlnsAggregattion : ContentPage
{
	public XmlnsAggregattion() => InitializeComponent();
	public XmlnsAggregattion(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void XamlWithAggregatedXmlns([Values] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(XmlnsAggregattion));

			var layout = new XmlnsAggregattion(useCompiledXaml);
			Assert.That(layout.label.Text, Is.EqualTo("Welcome to .NET MAUI!"));

		}
	}
}