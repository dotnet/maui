using System;
using System.IO;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Benchmark : ContentPage
{
	public Benchmark() => InitializeComponent();

	public void MockGenerationXamlC()
	{
		MockCompiler.Compile(typeof(FontSize), out var methodDef, out var hasLoggedErrors);
	}
	
	public void MockSourceGen()
	{
		var resourceId = XamlResourceIdAttribute.GetResourceIdForType(typeof(Benchmark));
		var resourcePath = XamlResourceIdAttribute.GetPathForType(typeof(Benchmark));
		var resourceStream = typeof(MockSourceGenerator).Assembly.GetManifestResourceStream(resourceId);

		MockSourceGenerator.RunMauiSourceGenerator(CreateMauiCompilation(), new AdditionalXamlFile(resourcePath, new StreamReader(resourceStream!).ReadToEnd()));
	}
	public void MockSourceGenLazy()
	{
		var resourceId = XamlResourceIdAttribute.GetResourceIdForType(typeof(Benchmark));
		var resourcePath = XamlResourceIdAttribute.GetPathForType(typeof(Benchmark));
		var resourceStream = typeof(MockSourceGenerator).Assembly.GetManifestResourceStream(resourceId);

		MockSourceGenerator.RunMauiSourceGenerator(CreateMauiCompilation(), new AdditionalXamlFile(resourcePath, new StreamReader(resourceStream!).ReadToEnd(), LazyOrder: true));
	}
}
