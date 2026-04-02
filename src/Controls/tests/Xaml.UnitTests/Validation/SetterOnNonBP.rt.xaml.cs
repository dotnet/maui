using Microsoft.Maui.Controls.Xaml;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class FakeView : View
{
	public string NonBindable { get; set; }
}

[Collection("Xaml Inflation feature")]
public partial class SetterOnNonBP : ContentPage
{
	public SetterOnNonBP() => InitializeComponent();

	public class SetterOnNonBPTests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ShouldThrow(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				XamlExceptionAssert.ThrowsBuildException(10, 13, () => MockCompiler.Compile(typeof(SetterOnNonBP)));
			else if (inflator == XamlInflator.Runtime)
				XamlExceptionAssert.ThrowsXamlParseException(10, 13, () => new SetterOnNonBP(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class FakeView : View
{
	public string NonBindable { get; set; }
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class SetterOnNonBP : ContentPage
{
	public SetterOnNonBP() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(SetterOnNonBP));
				Assert.NotEmpty(result.Diagnostics);
			}
			// else - unknown inflator, nothing to test
		}
	}
}