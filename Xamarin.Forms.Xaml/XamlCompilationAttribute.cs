using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	[Flags]
	public enum XamlCompilationOptions
	{
		Skip = 1 << 0,
		Compile = 1 << 1
	}

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
	public sealed class XamlCompilationAttribute : Attribute
	{
		public XamlCompilationAttribute(XamlCompilationOptions xamlCompilationOptions)
		{
			XamlCompilationOptions = xamlCompilationOptions;
		}

		public XamlCompilationOptions XamlCompilationOptions { get; set; }
	}

	static class XamlCExtensions
	{
		public static bool IsCompiled(this Type type)
		{
			var attr = type.GetTypeInfo().GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.Compile;
			attr = type.GetTypeInfo().Module.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.Compile;
			attr = type.GetTypeInfo().Assembly.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.Compile;

			return false;
		}
	}
}