using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	// TODO: obsolete when we are ready to switch to SourceGen
	//[Obsolete("use XamlProcessingOptions instead.")]
	[Flags]
	public enum XamlCompilationOptions
	{
		Skip = XamlInflator.Runtime,
		Compile = XamlInflator.XamlC,
	}

	// TODO: obsolete when we are ready to switch to SourceGen
	//[Obsolete("use XamlProcessingAttribute instead.")]
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
	public sealed class XamlCompilationAttribute : Attribute
	{
		public XamlCompilationAttribute(XamlCompilationOptions xamlCompilationOptions) 
		{ 
			XamlCompilationOptions = xamlCompilationOptions;
		}

		public XamlCompilationOptions XamlCompilationOptions { get; set;}
	}

	static class XamlCExtensions
	{
		public static bool IsCompiled(this Type type)
		{
#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

		}
	}
}