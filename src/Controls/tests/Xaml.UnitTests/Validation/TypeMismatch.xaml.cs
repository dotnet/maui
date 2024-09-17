using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class TypeMismatch : ContentPage
{
	public TypeMismatch() => InitializeComponent();

	public class Tests
	{
		[Test]
		public void ThrowsOnMismatchingType([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 16, m => m.Contains("No property, BindableProperty", StringComparison.Ordinal)), () => MockCompiler.Compile(typeof(TypeMismatch)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 16, m => m.StartsWith("Cannot assign property", StringComparison.Ordinal)), () => new TypeMismatch(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(TypeMismatch));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}