using System;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TypeMismatch : ContentPage
{
	public TypeMismatch() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void ThrowsOnMismatchingType(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(TypeMismatch))); // TODO: Was BuildExceptionConstraint(7, 16, ...)
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new TypeMismatch(inflator)); // TODO: Was XamlParseExceptionConstraint(7, 16, ...)
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
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}