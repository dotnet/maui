using System;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Benchmark2 : ContentPage
{
	public Benchmark2(string inflator)
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
