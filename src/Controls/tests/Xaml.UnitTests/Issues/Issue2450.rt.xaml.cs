using System;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

//this covers Issue2125 as well
public partial class Issue2450 : ContentPage
{
	public Issue2450() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ThrowMeaningfulExceptionOnDuplicateXName([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
			{
				var layout = new Issue2450(inflator);
				Assert.Throws(new XamlParseExceptionConstraint(11, 13, m => m == "An element with the name \"label0\" already exists in this NameScope"),
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
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Issue2450)));
		}
	}
}