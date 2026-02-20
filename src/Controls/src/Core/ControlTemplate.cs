#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>Defines the appearance of the templated control.</summary>
	public class ControlTemplate : ElementTemplate
	{
		/// <summary>Creates a new empty ControlTemplate.</summary>
		public ControlTemplate()
		{
		}

		/// <summary>Creates a new ControlTemplate with the specified type.</summary>
		/// <param name="type">The type to use as the template content.</param>
		public ControlTemplate(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
			: base(type)
		{
		}

		/// <summary>Creates a new ControlTemplate with the specified content factory.</summary>
		/// <param name="createTemplate">A factory method that creates the template content.</param>
		public ControlTemplate(Func<object> createTemplate) : base(createTemplate)
		{
		}
	}
}