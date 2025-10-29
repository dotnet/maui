using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui7744 : ContentPage
{
	public Maui7744() => InitializeComponent();
	public Maui7744(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
			public void Method(bool useCompiledXaml)
		{
			var page = new Maui7744(useCompiledXaml);
			Assert.IsType<RoundRectangle>(page.border0.StrokeShape);
			Assert.IsType<RoundRectangle>(page.border1.StrokeShape);
		}
	}
}