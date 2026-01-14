using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	[Obsolete("Specify xaml inflator and other options using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />", error: true)]
	[Flags]
	public enum XamlCompilationOptions
	{
		Skip = XamlInflator.Runtime,
		Compile = XamlInflator.XamlC,
	}

	[Obsolete("Specify xaml inflator and other options using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />", error: true)]
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
	public sealed class XamlCompilationAttribute : Attribute
	{
		public XamlCompilationAttribute(XamlCompilationOptions xamlCompilationOptions)
		{
			XamlCompilationOptions = xamlCompilationOptions;
		}

		public XamlCompilationOptions XamlCompilationOptions { get; set; }
	}
}