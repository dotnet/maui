using Microsoft.Maui.Controls.Xaml;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class FakeView : View
{
	public string NonBindable { get; set; }
}

public partial class SetterOnNonBP : ContentPage
{
	public SetterOnNonBP() => InitializeComponent();

	public class SetterOnNonBPTests
	{
		[Theory]
		[Values]
		public void ShouldThrow(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				BuildExceptionHelper.AssertThrows(() => MockCompiler.Compile(typeof(SetterOnNonBP)), 10, 13);
			else if (inflator == XamlInflator.Runtime)
				XamlParseExceptionHelper.AssertThrows(() => new SetterOnNonBP(inflator), 10, 13);
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
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}