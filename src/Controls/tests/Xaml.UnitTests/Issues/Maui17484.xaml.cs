using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17484 : ContentPage
{
	public Maui17484() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ObsoleteinDT([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui17484)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17484));
				Assert.That(result.Diagnostics, Is.Empty);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.DoesNotThrow(() => new Maui17484(inflator));
			else
				Assert.Ignore("Only XamlC, SourceGen and Runtime are supported for this test");
		}
	}
}