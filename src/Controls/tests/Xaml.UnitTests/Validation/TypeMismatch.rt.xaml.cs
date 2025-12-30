using System;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation feature")]
public partial class TypeMismatch : ContentPage
{
	public TypeMismatch() => InitializeComponent();

	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ThrowsOnMismatchingType(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				XamlExceptionAssert.ThrowsBuildException(7, 16, m => m.Contains("No property, BindableProperty", StringComparison.Ordinal), () => MockCompiler.Compile(typeof(TypeMismatch)));
			else if (inflator == XamlInflator.Runtime)
				XamlExceptionAssert.ThrowsXamlParseException(7, 16, m => m.StartsWith("Cannot assign property", StringComparison.Ordinal), () => new TypeMismatch(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class TypeMismatch : ContentPage
{
	public TypeMismatch() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(TypeMismatch));
				Assert.NotEmpty(result.Diagnostics);
			}
			// else - unknown inflator, nothing to test
		}
	}
}