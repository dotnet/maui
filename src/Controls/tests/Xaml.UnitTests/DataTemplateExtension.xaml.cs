using System;
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DataTemplateExtension : ContentPage
{
	public DataTemplateExtension() => InitializeComponent();

	public class Tests
	{

		[Theory]
		[Values]
		public void DataTemplateExtension(XamlInflator inflator)
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

		[Xunit.Fact(Skip = "Test was not running in NUnit (no [Test] attribute) - needs investigation")]
		public void ExtensionsAreReplaced()
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
			Assert.True(initComp.Contains("typeof(global::Microsoft.Maui.Controls.Xaml.UnitTests.DataTemplateExtension)", StringComparison.InvariantCulture));
			Assert.DoesNotContain("ProvideValue", initComp);
		}
	}
}