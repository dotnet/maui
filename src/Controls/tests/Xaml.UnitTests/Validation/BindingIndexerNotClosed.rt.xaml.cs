using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BindingIndexerNotClosed : ContentPage
{
public BindingIndexerNotClosed() => InitializeComponent();

[TestFixture]
class Tests
{
[Test]
public void ThrowsOnUnclosedIndexer([Values] XamlInflator inflator)
{
if (inflator == XamlInflator.XamlC)
Assert.Throws(new BuildExceptionConstraint(5, 17), () => MockCompiler.Compile(typeof(BindingIndexerNotClosed)));
else if (inflator == XamlInflator.Runtime)
Assert.Throws(new XamlParseExceptionConstraint(5, 17), () => new BindingIndexerNotClosed(inflator));
else if (inflator == XamlInflator.SourceGen)
{
var result = CreateMauiCompilation()
.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class BindingIndexerNotClosed : ContentPage
{
public BindingIndexerNotClosed() => InitializeComponent();
}
""")
.RunMauiSourceGenerator(typeof(BindingIndexerNotClosed));
Assert.That(result.Diagnostics, Is.Not.Empty);
Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2041"), Is.True);
}
else
Assert.Ignore($"Unknown inflator {inflator}");
}
}
}
