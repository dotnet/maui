using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XSharedNotSupported : ContentPage
{
	public XSharedNotSupported() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void XSharedThrowsOnRuntimeAndXamlC(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
			{
				// x:Shared is only supported with SourceGen - Runtime should throw
				var ex = Assert.Throws<XamlParseException>(() => new XSharedNotSupported(inflator));
				Assert.Contains("x:Shared", ex.Message, StringComparison.Ordinal);
			}
			else if (inflator == XamlInflator.XamlC)
			{
				// XamlC should also throw for x:Shared
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(XSharedNotSupported)));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// SourceGen supports x:Shared - tested in LazyResourceDictionary.xaml.cs
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class XSharedNotSupported : ContentPage
{
	public XSharedNotSupported() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(XSharedNotSupported));
				// SourceGen should compile without errors
				Assert.Empty(result.Diagnostics);
			}
		}
	}
}
