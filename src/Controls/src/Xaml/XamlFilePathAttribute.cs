using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Specifies the file path of the XAML file associated with a type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class XamlFilePathAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XamlFilePathAttribute"/> with the file path of the caller.
		/// </summary>
		/// <param name="filePath">The file path, automatically set by the compiler.</param>
		public XamlFilePathAttribute([CallerFilePath] string filePath = "") => FilePath = filePath;

		/// <summary>
		/// Gets the file path of the XAML file.
		/// </summary>
		public string FilePath { get; }

		internal static string GetFilePathForObject(object view) => (view?.GetType().GetCustomAttributes(typeof(XamlFilePathAttribute), false).FirstOrDefault() as XamlFilePathAttribute)?.FilePath;
	}
}