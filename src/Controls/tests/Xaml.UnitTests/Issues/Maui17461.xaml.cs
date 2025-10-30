using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17461 : ContentPage
{

	public Maui17461() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void MissingKeyException([Values("net7.0-ios", "net7.0-android", "net7.0-macos")] string targetFramework, [Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(Maui17461), out var methodDef, targetFramework: targetFramework);
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17461));
				Assert.That(result.Diagnostics, Is.Empty);
			}
			else
				Assert.Ignore("Only XamlC and SourceGen are supported for this test");
		}
	}
}