using System;
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
		MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Benchmark));
	}
}
