using NUnit.Framework;

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
	}
}