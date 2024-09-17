// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class xKeyLiteral : ContentPage
{
	public xKeyLiteral() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		//this requirement might change, see https://github.com/xamarin/Microsoft.Maui.Controls/issues/12425
		[Test]
		public void xKeyRequireStringLiteral([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(xKeyLiteral)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new xKeyLiteral(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.CreateMauiCompilation().RunMauiSourceGenerator(typeof(xKeyLiteral));
				Assert.That(result.Diagnostics.Any());
			}
			else
				Assert.Ignore("Unknown inflator");
		}
	}
}
