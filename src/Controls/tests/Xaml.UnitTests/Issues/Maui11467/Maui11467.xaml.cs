using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11467 : ParentButton
{
	public Maui11467() => InitializeComponent();

	public Maui11467(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Method(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui11467));

			// Used to throw:
			// XamlParseException : Position 5:9. No method ParentButton_OnClicked with correct signature found on type Microsoft.Maui.Controls.Xaml.UnitTests.Maui11467
			var button = new Maui11467(useCompiledXaml);
			Assert.Equal(1, button.MyEventSubscriberCount);
		}
	}
}