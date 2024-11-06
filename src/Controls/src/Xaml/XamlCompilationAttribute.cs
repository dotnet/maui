using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	public enum XamlCompilationOptions
	{
		/// <summary>
		/// Picks the best Inflator available, don't change it unless you know what you're doing.
		/// </summary>
		Default = 0,
		RuntimeInflator = 1 << 0,
		[Obsolete("use Default. Or use RuntimeInflator to force XAML inflation at runtime.")]
		Skip = RuntimeInflator,
		XamlCInflator = 1 << 1,
		[Obsolete("use Default. Or use XamlCInflator to force XAML inflation using XamlC at compile time.")]
		Compile = XamlCInflator,
		SourceGenInflator = 1 << 2,
	}

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class, Inherited = false)]
	public sealed class XamlCompilationAttribute : Attribute
	{
		public XamlCompilationAttribute(XamlCompilationOptions xamlCompilationOptions) : this(xamlCompilationOptions, false)
		{
		}
		internal XamlCompilationAttribute(XamlCompilationOptions inflators, bool generateDebugSwitch)
		{ 
			GenerateDebugSwitch = generateDebugSwitch;
			XamlCompilationOptions = inflators;
		}

		public XamlCompilationOptions XamlCompilationOptions { get; }
		internal bool GenerateDebugSwitch { get; } = false;
	}

	static class XamlCExtensions
	{
		public static bool IsCompiled(this Type type)
		{
			var attr = type.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.XamlCInflator;
			attr = type.Module.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.XamlCInflator;
			attr = type.Assembly.GetCustomAttribute<XamlCompilationAttribute>();
			if (attr != null)
				return attr.XamlCompilationOptions == XamlCompilationOptions.XamlCInflator;

			return false;
		}
	}
}