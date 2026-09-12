using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
#if NET12_0_OR_GREATER
	[Obsolete("XamlCompilationOptions is no longer used. Specify xaml inflator using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />", error: true)]
#else
	[Obsolete("XamlCompilationOptions is deprecated. Specify xaml inflator using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />")]
#endif
	[Flags]
	public enum XamlCompilationOptions
	{
		Skip = XamlInflator.Runtime,
		Compile = XamlInflator.XamlC,
	}

#if NET12_0_OR_GREATER
	[Obsolete("XamlCompilationAttribute is no longer used. Specify xaml inflator using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />", error: true)]
#else
	[Obsolete("XamlCompilationAttribute is deprecated. Specify xaml inflator using msbuild metadata on MauiXaml items in your .csproj: <MauiXaml Update=\"YourFile.xaml\" Inflator=\"XamlC\" />")]
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

#if !NET12_0_OR_GREATER
#pragma warning disable CS0618 // Type or member is obsolete - internal backcompat code
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
#pragma warning restore CS0618
#endif
}