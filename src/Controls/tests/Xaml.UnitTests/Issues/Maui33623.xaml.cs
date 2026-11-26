using System;
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33623 : ContentPage
{
	public Maui33623() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void NamedFontSizeEmitsObsoleteWarning(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Maui33623), out _, out var hasLoggedErrors, out var warnings, treatWarningsAsErrors: false);
				Assert.False(hasLoggedErrors);

				var obsoleteWarnings = warnings.Where(w => w.Code == "XC0618").ToArray();
				Assert.Equal(2, obsoleteWarnings.Length);
				Assert.Contains(obsoleteWarnings, w => w.Message.Contains("NamedSize.Large", StringComparison.Ordinal) && w.LineNumber == 6);
				Assert.Contains(obsoleteWarnings, w => w.Message.Contains("NamedSize.Medium", StringComparison.Ordinal) && w.LineNumber == 8);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui33623 : ContentPage
{
	public Maui33623() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33623));

				var obsoleteWarnings = result.Diagnostics.Where(d => d.Id == "MAUIX2025").ToArray();
				Assert.Equal(2, obsoleteWarnings.Length);
				Assert.Contains(obsoleteWarnings, d => d.GetMessage().Contains("NamedSize.Large", StringComparison.Ordinal));
				Assert.Contains(obsoleteWarnings, d => d.GetMessage().Contains("NamedSize.Medium", StringComparison.Ordinal));
			}
		}

		[Fact]
		internal void NamedFontSizeWarningBecomesErrorWhenTreatWarningsAsErrors()
		{
			var ex = Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(Maui33623), treatWarningsAsErrors: true));
			Assert.NotNull(ex);
		}

		[Theory]
		[XamlInflatorData]
		internal void NamedFontSizeStillInflatesCorrectly(XamlInflator inflator)
		{
			var page = new Maui33623(inflator);
			Assert.Equal(24, page.numericLabel.FontSize);

#pragma warning disable CS0612 // Type or member is obsolete
			Assert.Equal(Device.GetNamedSize(NamedSize.Large, page.largeLabel), page.largeLabel.FontSize);
			Assert.Equal(Device.GetNamedSize(NamedSize.Medium, page.mediumButton), page.mediumButton.FontSize);
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}
}
