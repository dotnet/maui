using System;
using System.Linq;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DataTemplateExtension : ContentPage
{
	public DataTemplateExtension() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void DataTemplateExtension([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(DataTemplateExtension));
			}
			var layout = new DataTemplateExtension(inflator);
			var content = layout.Resources["content"] as ShellContent;
			var template = content.ContentTemplate;
			var obj = template.CreateContent();
			Assert.That(obj, Is.TypeOf<DataTemplateExtension>());
		}

				public void ExtensionsAreReplaced([Values(XamlInflator.SourceGen)] XamlInflator inflator)
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DataTemplateExtension : ContentPage
{
	public DataTemplateExtension() => InitializeComponent();
		}
""")
				.RunMauiSourceGenerator(typeof(DataTemplateExtension));
			Assert.IsFalse(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			Assert.That(initComp.Contains("typeof(global::Microsoft.Maui.Controls.Xaml.UnitTests.DataTemplateExtension)", StringComparison.InvariantCulture));
			Assert.That(initComp, Does.Not.Contains("ProvideValue"));
		}
	}
}