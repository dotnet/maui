using System.Linq;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DuplicatePropertyElements : BindableObject
{
	public DuplicatePropertyElements() => InitializeComponent();

	[TestFixture]
	static class Tests
	{
		[Test]
		public static void ThrowXamlParseException([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(DuplicatePropertyElements), out var md, out var hasLoggedErrors);
				Assert.That(hasLoggedErrors);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new DuplicatePropertyElements(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class DuplicatePropertyElements : BindableObject
{
	public DuplicatePropertyElements() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(DuplicatePropertyElements));

				Assert.That(result.Diagnostics.Any());
			}
		}
	}
}
