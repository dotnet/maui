using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui8149 : ContentView
{

	public Maui8149() => InitializeComponent();

	public Maui8149(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Fact]
		public void NamescopeWithXamlC([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui8149));

			var page = new Maui8149(useCompiledXaml);
			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests.Maui8149", (page.Content as Maui8149View).Text);
		}
	}
}