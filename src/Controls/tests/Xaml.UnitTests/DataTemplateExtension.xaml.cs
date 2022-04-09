using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DataTemplateExtension : ContentPage
	{
		public DataTemplateExtension() => InitializeComponent();
		public DataTemplateExtension(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void DataTemplateExtension(bool useCompiledXaml)
			{
				var layout = new DataTemplateExtension(useCompiledXaml);
				var content = layout.Resources["content"] as ShellContent;
				var template = content.ContentTemplate;
				var obj = template.CreateContent();
				Assert.That(obj, Is.TypeOf<DataTemplateExtension>());
			}
		}
	}
}