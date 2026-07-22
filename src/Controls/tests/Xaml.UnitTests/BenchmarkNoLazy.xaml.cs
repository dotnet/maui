using System;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BenchmarkNoLazy : ContentPage
{
	public BenchmarkNoLazy(string inflator)
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
