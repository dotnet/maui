using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();


	public class Issue2115
	{
		[Theory]
		[Values]
		public void xStaticThrowsMeaningfullException(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
				XamlParseExceptionHelper.AssertThrows(() => new StaticExtensionException(inflator), 6, 34);
			else if (inflator == XamlInflator.XamlC)
				BuildExceptionHelper.AssertThrows(() => MockCompiler.Compile(typeof(StaticExtensionException)), 6, 34);
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(StaticExtensionException));
				Assert.True(result.Diagnostics.Any());
			}
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}