using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7187 : ContentPage
{
	public Gh7187() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void InvalidMarkupAssignmentThrowsXPE([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh7187)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh7187(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh7187 : ContentPage
{
	public Gh7187() => InitializeComponent();

""")
					.RunMauiSourceGenerator(typeof(Gh7187));
				//FIXME check diagnostic code
				Assert.That(result.Diagnostics.Length, Is.EqualTo(1));
			}
			else
				Assert.Ignore($"XamlInflator {inflator} not tested");

		}
	}
}
