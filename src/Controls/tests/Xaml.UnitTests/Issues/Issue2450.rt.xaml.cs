using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

//this covers Issue2125 as well
public partial class Issue2450 : ContentPage
{
	public Issue2450() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ThrowMeaningfulExceptionOnDuplicateXName(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
			{
				var layout = new Issue2450(inflator);
				Assert.Throws<XamlParseException>(
							() => (layout.Resources["foo"] as Microsoft.Maui.Controls.DataTemplate).CreateContent());
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Issue2450 : ContentPage
{
	public Issue2450() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Issue2450));
				//FIXME check diagnostic code
				Assert.NotEmpty(result.Diagnostics);
			}
			else if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Issue2450)));
		}
	}
}