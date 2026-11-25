using System;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[AcceptEmptyServiceProvider]
public class Maui37936MarkupExtension : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider) => this;
}

public partial class Maui37936 : Button
{
	public Maui37936() => InitializeComponent();

	public Maui37936MarkupExtension DefaultMarkupExtension { get; set; }

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{

		[Fact]
		internal void OnPlatformWithMissingTargetPlatformShouldNotInvokeProvideValueInSourceGen()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using System;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[global::Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProvider]
public class Maui37936MarkupExtension : global::Microsoft.Maui.Controls.Xaml.IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider) => this;
}

[global::Microsoft.Maui.Controls.Xaml.XamlProcessing(global::Microsoft.Maui.Controls.Xaml.XamlInflator.Runtime, true)]
public partial class Maui37936 : global::Microsoft.Maui.Controls.Button
{
	public Maui37936MarkupExtension DefaultMarkupExtension { get; set; }

	public Maui37936() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(Maui37936), targetFramework: "net10.0-ios");

			Assert.Empty(result.Diagnostics);
			Assert.DoesNotContain(".ProvideValue(", result.GeneratedInitializeComponent(), StringComparison.Ordinal);
		}
	}
}
