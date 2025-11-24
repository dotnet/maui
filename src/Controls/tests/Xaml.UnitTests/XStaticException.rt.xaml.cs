using System.Linq;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XStaticException : ContentPage
{
	public XStaticException() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		//{x:Static Member=prefix:typeName.staticMemberName}
		//{x:Static prefix:typeName.staticMemberName}

		//The code entity that is referenced must be one of the following:
		// - A constant
		// - A static property
		// - A field
		// - An enumeration value
		// All other cases should throw

		[Test]
		public void ThrowOnInstanceProperty([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 6), () => MockCompiler.Compile(typeof(XStaticException)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws(new XamlParseExceptionConstraint(7, 6), () => new XStaticException(inflator));
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
				Assert.That(!result.Diagnostics.Any());
			}
			else
				Assert.Ignore("Unknown inflator");
		}
	}
}