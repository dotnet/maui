using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XStaticException : ContentPage
{
	public XStaticException() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		//{x:Static Member=prefix:typeName.staticMemberName}
		//{x:Static prefix:typeName.staticMemberName}

		//The code entity that is referenced must be one of the following:
		// - A constant
		// - A static property
		// - A field
		// - An enumeration value
		// All other cases should throw

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void ThrowOnInstanceProperty(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(XStaticException)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new XStaticException(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class XStaticException : ContentPage
{
	public XStaticException() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(XStaticException));
				// When the type is not in the compilation (as is the case here),
				// SourceGen falls back to letting the C# compiler handle the validation.
				// This is intentional behavior for unresolved types in clr-namespaces.
				Assert.False(result.Diagnostics.Any());
			}
		}
	}
}