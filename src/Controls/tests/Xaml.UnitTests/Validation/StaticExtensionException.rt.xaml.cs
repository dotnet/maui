using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();

	[Collection("Issue")]
	public class Issue2115 : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void xStaticThrowsMeaningfullException(XamlInflator inflator)
		{
			if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new StaticExtensionException(inflator));
			else if (inflator == XamlInflator.XamlC)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(StaticExtensionException)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(StaticExtensionException));
				Assert.True(result.Diagnostics.Any());
			}
		}

		[Fact]
		internal void SourceGenReportsMalformedStaticExtensionLocation()
		{
			const string xamlPath = "/Users/test/project/Views/HomePage.xaml";
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class MyCustomControl : ContentView
{
	public string Title { get; set; }
	public object IconSource { get; set; }
}

public partial class HomePage : ContentPage
{
	public HomePage() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(new AdditionalXamlFile(
					xamlPath,
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui" xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests" x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.HomePage">
	<local:MyCustomControl Title="Home" IconSource="{x:Static}"/>
</ContentPage>
""",
					RelativePath: "Views/HomePage.xaml",
					TargetPath: "Views/HomePage.xaml",
					ManifestResourceName: "Microsoft.Maui.Controls.Xaml.UnitTests.Views.HomePage.xaml"));

			var diagnostic = Assert.Single(result.Diagnostics.Where(diagnostic => diagnostic.Id == "MAUIG1001"));
			var lineSpan = diagnostic.Location.GetLineSpan();
			var message = diagnostic.GetMessage();

			Assert.Equal(xamlPath, lineSpan.Path);
			Assert.Equal(1, lineSpan.StartLinePosition.Line);
			Assert.Contains(xamlPath, message, System.StringComparison.Ordinal);
			Assert.Contains("Line: 2", message, System.StringComparison.Ordinal);
			Assert.Contains("IconSource", message, System.StringComparison.Ordinal);
			Assert.Contains("{x:Static}", message, System.StringComparison.Ordinal);
		}
	}
}