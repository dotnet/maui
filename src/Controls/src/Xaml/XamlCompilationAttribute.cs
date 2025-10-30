using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
#if !_MAUIXAML_SOURCEGEN_BACKCOMPAT
	[Obsolete("Specify xaml inflator and other options using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />", error: true)]
#endif
	[Flags]
	public enum XamlCompilationOptions
	{
		Skip = XamlInflator.Runtime,
		Compile = XamlInflator.XamlC,
	}

#if !_MAUIXAML_SOURCEGEN_BACKCOMPAT
	[Obsolete("Specify xaml inflator and other options using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />", error: true)]
#endif
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
	public sealed class XamlCompilationAttribute : Attribute
	{
		public XamlCompilationAttribute(XamlCompilationOptions xamlCompilationOptions)
		{
			XamlCompilationOptions = xamlCompilationOptions;
		}

		public XamlCompilationOptions XamlCompilationOptions { get; set; }
	}

#if _MAUIXAML_SOURCEGEN_BACKCOMPAT
	static class XamlCExtensions
	{
		public static bool IsCompiled(this Type type)
		{
			var attr = type.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.Compile;
			attr = type.Module.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.Compile;
			attr = type.Assembly.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.Compile;

			return false;
		}
	}
#endif
}