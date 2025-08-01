using System;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11061 : ContentPage
{
	public DateTime MyDateTime { get; set; }

	public Gh11061() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BindingOnNonBP([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh11061)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh11061 : ContentPage
{
	public DateTime MyDateTime { get; set; }

	public Gh11061() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh11061));
				Assert.That(result.Diagnostics, Is.Not.Empty);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Gh11061(inflator) { BindingContext = new { Date = DateTime.Today } });
		}
	}
}
