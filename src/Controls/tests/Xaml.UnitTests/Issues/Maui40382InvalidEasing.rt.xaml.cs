using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// A typo in an easing function is a build error under the source generator and XamlC, not a page-load failure.
public partial class Maui40382InvalidEasing : ContentPage
{
	public Maui40382InvalidEasing() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void UnknownEasingFunctionIsRejected(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui40382InvalidEasing : ContentPage
{
	public Maui40382InvalidEasing() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui40382InvalidEasing));
				Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("Sprung(0.7)", StringComparison.Ordinal));
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.ThrowsAny<Exception>(() => new Maui40382InvalidEasing(XamlInflator.Runtime));
			else if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Maui40382InvalidEasing)));
		}
	}
}
