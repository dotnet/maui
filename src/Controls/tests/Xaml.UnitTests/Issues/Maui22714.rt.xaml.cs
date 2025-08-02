using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22714
{
	public Maui22714() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void TestNonCompiledResourceDictionary(
			[Values] XamlInflator inflator,
			[Values] bool treatWarningsAsErrors)
		{
			if (inflator == XamlInflator.XamlC)
			{
				if (treatWarningsAsErrors)
				{
					Assert.Throws(
						new BuildExceptionConstraint(22, 32, static msg => msg.Contains(" XC0024 ", System.StringComparison.Ordinal)),
						() => MockCompiler.Compile(typeof(Maui22714), treatWarningsAsErrors: true));
				}
				else
				{
					MockCompiler.Compile(typeof(Maui22714), treatWarningsAsErrors: false);
				}
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui22714));
			}
			else if (inflator == XamlInflator.Runtime)
			{
				// This will never affect non-compiled builds
				_ = new Maui22714(inflator);
			}
			else
				Assert.Ignore("This test is not yet implemented");
		}
	}
}

public class PageViewModel22714
{
	public string Title { get; set; }
	public ItemViewModel22714[] Items { get; set; }
}

public class ItemViewModel22714
{
	public string Title { get; set; }
}
