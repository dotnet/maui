using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

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