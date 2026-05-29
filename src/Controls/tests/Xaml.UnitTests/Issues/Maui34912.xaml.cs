using System;
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
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class Maui34912 : ContentPage
{
	public Maui34912() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(Maui34912));

			Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2009");
			Assert.Empty(result.Diagnostics);

			var page = new Maui34912(XamlInflator.SourceGen);
			Assert.False(string.IsNullOrWhiteSpace(page.nowLabel.Text));
			Assert.False(string.IsNullOrWhiteSpace(page.utcNowLabel.Text));
		}
	}
}
