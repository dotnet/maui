
namespace Microsoft.Maui.IntegrationTests
{
	public class AppleTemplateTests : BaseBuildTest
	{
		[OneTimeSetUp]
		public void AppleTemplateFxtSetup()
		{
		}

		[OneTimeTearDown]
		public void AppleTemplateFxtTearDown()
		{

		}

		[Test]
		[TestCase("maui", "Debug", "net6.0")]
		[TestCase("maui", "Release", "net6.0")]
		[TestCase("maui", "Debug", "net7.0")]
		[TestCase("maui", "Release", "net7.0")]
		[TestCase("maui-blazor", "Debug", "net6.0")]
		[TestCase("maui-blazor", "Release", "net6.0")]
		[TestCase("maui-blazor", "Debug", "net7.0")]
		[TestCase("maui-blazor", "Release", "net7.0")]
		public void RunOniOS(string id, string framework, string config)
		{
			if (!TestEnvironment.IsMacOS)
				Assert.Ignore("iOS run template tests only run on macOS.");

			//TODO
		}

	}
}
