using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DuplicateXArgumentsElements : BindableObject
{
	public DuplicateXArgumentsElements() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void ThrowXamlParseException(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new DuplicateXArgumentsElements(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class DuplicateXArgumentsElements : BindableObject
{
	public DuplicateXArgumentsElements() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(DuplicateXArgumentsElements));
				Assert.True(result.Diagnostics.Any());
			}
		}
	}
}
