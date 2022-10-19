using System;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Attribute to indicate that the adorned class can be compiled through XAML compilation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	sealed class ProvideCompiledAttribute : Attribute
	{
		/// <summary>
		/// The fully qualified type name to provide a compiled version for.
		/// </summary>
		public string CompiledVersion { get; }

		/// <summary>
		/// Creates a new <see cref="ProvideCompiledAttribute"/> object.
		/// </summary>
		/// <param name="compiledVersion"></param>
		public ProvideCompiledAttribute(string compiledVersion)
		{
			CompiledVersion = compiledVersion;
		}
	}
}