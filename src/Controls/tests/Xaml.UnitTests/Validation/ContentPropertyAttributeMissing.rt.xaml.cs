using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class TypeWithoutContentProperty : View
{
}

public partial class ContentPropertyAttributeMissing : ContentPage
{
	public ContentPropertyAttributeMissing() => InitializeComponent();

	[TestFixture]
class Tests
	{
		[Test]
		public void ThrowsOnMissingContentPropertyAttribute([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 3), () => MockCompiler.Compile(typeof(ContentPropertyAttributeMissing)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 3), () => new ContentPropertyAttributeMissing(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class TypeWithoutContentProperty : View
{
}

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class ContentPropertyAttributeMissing : ContentPage
{
	public ContentPropertyAttributeMissing() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(ContentPropertyAttributeMissing));
				Assert.That(result.Diagnostics, Is.Not.Empty);
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2065"), Is.True);
			}
			else
				Assert.Ignore($"Unknown inflator {inflator}");
		}
	}
}
