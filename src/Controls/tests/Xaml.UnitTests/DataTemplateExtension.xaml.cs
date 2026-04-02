using System;
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DataTemplateExtension : ContentPage
{
	public DataTemplateExtension() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void DataTemplateExtensionTest(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(DataTemplateExtension));
			}
			var layout = new DataTemplateExtension(inflator);
			var content = layout.Resources["content"] as ShellContent;
			var template = content.ContentTemplate;
			var obj = template.CreateContent();
			Assert.IsType<DataTemplateExtension>(obj);
		}

		[Fact]
		internal void ExtensionsAreReplaced()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DataTemplateExtension : ContentPage
{
	public DataTemplateExtension() => InitializeComponent();
		}
""")
				.RunMauiSourceGenerator(typeof(DataTemplateExtension));
			Assert.False(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			Assert.Contains("typeof(global::Microsoft.Maui.Controls.Xaml.UnitTests.DataTemplateExtension)", initComp, StringComparison.Ordinal);
			Assert.DoesNotContain("ProvideValue", initComp, StringComparison.Ordinal);
		}
	}
}