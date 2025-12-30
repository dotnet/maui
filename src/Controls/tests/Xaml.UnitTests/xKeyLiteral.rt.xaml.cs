// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class xKeyLiteral : ContentPage
{
	public xKeyLiteral() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		//this requirement might change, see https://github.com/xamarin/Xamarin.Forms/issues/12425
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void xKeyRequireStringLiteral(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(xKeyLiteral)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new xKeyLiteral(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class xKeyLiteral : ContentPage
{
	public xKeyLiteral() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(xKeyLiteral));
				Assert.NotEmpty(result.Diagnostics);
			}
		}
	}
}
