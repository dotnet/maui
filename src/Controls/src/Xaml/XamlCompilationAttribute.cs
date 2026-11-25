using System;

namespace Microsoft.Maui.Controls.Xaml
{
#if NET12_0_OR_GREATER
	[Obsolete("XamlCompilationOptions is no longer used. Use the default SourceGen inflator, or specify Inflator=\"Runtime\" on MauiXaml items in your .csproj.", error: true)]
#else
	[Obsolete("XamlCompilationOptions is deprecated and no longer used. Use the default SourceGen inflator, or specify Inflator=\"Runtime\" on MauiXaml items in your .csproj.")]
#endif
	[Flags]
	public enum XamlCompilationOptions
	{
		Skip = XamlInflator.Runtime,
		Compile = 1 << 1,
	}

#if NET12_0_OR_GREATER
	[Obsolete("XamlCompilationAttribute is no longer used. Use the default SourceGen inflator, or specify Inflator=\"Runtime\" on MauiXaml items in your .csproj.", error: true)]
#else
	[Obsolete("XamlCompilationAttribute is deprecated and no longer used. Use the default SourceGen inflator, or specify Inflator=\"Runtime\" on MauiXaml items in your .csproj.")]
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
}