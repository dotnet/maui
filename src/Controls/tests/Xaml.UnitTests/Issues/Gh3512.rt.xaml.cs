using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3512 : ContentPage
{
	public Gh3512() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ThrowsOnDuplicateXKey(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh3512)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<ArgumentException>(() => new Gh3512(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Gh3512 : ContentPage
{
	public Gh3512() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh3512));
				Assert.Single(result.Diagnostics);
			}
		}
	}
}
