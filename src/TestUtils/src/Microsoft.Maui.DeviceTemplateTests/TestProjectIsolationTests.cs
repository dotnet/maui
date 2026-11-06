namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class TestProjectIsolationTests
{
	[Theory]
	[InlineData("AppleTemplateLaunchProbe.cs")]
	[InlineData("TemplateLaunchInstrumentation.cs")]
	public void LaunchResourcesBelongToDeviceTestAssembly(string resourceName)
	{
		var assembly = typeof(FileUtilities).Assembly;
		Assert.Equal(typeof(TestProjectIsolationTests).Assembly, assembly);
		using var resource = assembly.GetManifestResourceStream(resourceName);
		Assert.NotNull(resource);
	}

	[Fact]
	public void SharedHelpersDoNotReferenceIntegrationTestAssembly()
	{
		var assembly = typeof(TestProjectIsolationTests).Assembly;
		Assert.Equal(assembly, typeof(BaseBuildTest).Assembly);
		Assert.Equal(assembly, typeof(BaseTemplateTests).Assembly);
		Assert.DoesNotContain(assembly.GetReferencedAssemblies(),
			reference => reference.Name == "Microsoft.Maui.IntegrationTests");
	}

	[Fact]
	public void GeneratedProjectsUseDeviceSpecificDirectory()
	{
		Assert.Equal("device-template-test-dir", Path.GetFileName(TestEnvironment.GetTestDirectoryRoot()));
	}
}
