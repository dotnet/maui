using System;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Benchmark : ContentPage
{
	public Benchmark(string inflator)
	{
		switch (inflator)
		{
			case "Runtime":
				InitializeComponentRuntime();
				break;
			case "XamlC":
				InitializeComponentXamlC();
				break;
			case "SourceGen":
				InitializeComponentSourceGen();
				break;
			default:
				throw new NotSupportedException($"no code for {inflator} generated. check the [XamlProcessing] attribute.");
		}
	}
}
