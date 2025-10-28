using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui22714
{
	public Maui22714()
	{
		InitializeComponent();
	}

	public Maui22714(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Fact]
		public void TestNonCompiledResourceDictionary(
			[InlineData(false, true)] bool useCompiledXaml,
			[InlineData(false, true)] bool treatWarningsAsErrors)
		{
			if (useCompiledXaml)
			{
				if (treatWarningsAsErrors)
				{
					new BuildExceptionConstraint(22, 32, static msg => msg.Contains(" XC0024 ", System.StringComparison.Ordinal)).Validate(
						() => MockCompiler.Compile(typeof(Maui22714), treatWarningsAsErrors: true));
				}
				else
				{
					MockCompiler.Compile(typeof(Maui22714), treatWarningsAsErrors: false);
				}
			}
			else
			{
				// This will never affect non-compiled builds
				_ = new Maui22714(useCompiledXaml);
			}
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
