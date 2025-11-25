using System.Linq;

using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BindingIndexerEmpty : ContentPage
{
public BindingIndexerEmpty() => InitializeComponent();

[TestFixture]
class Tests
{
[Test]
public void ThrowsOnEmptyIndexer([Values] XamlInflator inflator)
{
if (inflator == XamlInflator.XamlC)
Assert.Throws(new BuildExceptionConstraint(5, 17), () => MockCompiler.Compile(typeof(BindingIndexerEmpty)));
else if (inflator == XamlInflator.Runtime)
Assert.Throws(new XamlParseExceptionConstraint(5, 17), () => new BindingIndexerEmpty(inflator));
else if (inflator == XamlInflator.SourceGen)
{
var result = CreateMauiCompilation()
.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class BindingIndexerEmpty : ContentPage
{
public BindingIndexerEmpty() => InitializeComponent();
}
""")
.RunMauiSourceGenerator(typeof(BindingIndexerEmpty));
Assert.That(result.Diagnostics, Is.Not.Empty);
Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2042"), Is.True);
}
else
Assert.Ignore($"Unknown inflator {inflator}");
}
}
}
