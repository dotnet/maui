#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies an assembly-level mapping between a visual key name and an <see cref="IVisual"/> type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class VisualAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VisualAttribute"/> class with the specified key and visual type.
		/// </summary>
		/// <param name="key">The key name used to reference the visual in XAML.</param>
		/// <param name="visual">The <see cref="IVisual"/> type to associate with the key.</param>
		public VisualAttribute(
			string key,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type visual)
		{
			this.Key = key;
			this.Visual = visual;
		}

		internal string Key { get; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal Type Visual { get; }
	}
}
