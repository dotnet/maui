global using System;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34912 : ContentPage
{
	public Maui34912() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Fact]
		public void StaticTypeInvocationRespectsGlobalUsings()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
global using System;
""", "GlobalUsings.cs")
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui34912ViewModel
{
}

[XamlProcessing(XamlInflator.SourceGen)]
public partial class Maui34912 : ContentPage
{
	public Maui34912() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(
					new AdditionalXamlFile(
						"Issues/Maui34912.sgen.xaml",
						System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetThisFilePath()), "Maui34912.sgen.xaml"))));

			Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2009");
			Assert.Empty(result.Diagnostics);

			var page = new Maui34912(XamlInflator.SourceGen);
			Assert.False(string.IsNullOrWhiteSpace(page.nowLabel.Text));
			Assert.False(string.IsNullOrWhiteSpace(page.utcNowLabel.Text));
		}

		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;
	}
}

public class Maui34912ViewModel
{
}
