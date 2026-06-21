#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
#pragma warning disable CS1734 // XML comment on 'ExportEffectAttribute' has a paramref tag for 'effectType', but there is no parameter by that name
	/// <summary>Attribute that identifies a <see cref="Microsoft.Maui.Controls.Effect"/> with a unique identifier that can be used with <see cref="M:Microsoft.Maui.Controls.Effect.Resolve(System.String)"/> to locate an effect.</summary>
#pragma warning restore CS1734
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportEffectAttribute : Attribute
	{
		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.ExportEffectAttribute"/>.</summary>
		/// <param name="effectType">The type of the marked <see cref="Microsoft.Maui.Controls.Effect"/>.</param>
		/// <param name="uniqueName">A unique name for the <see cref="Microsoft.Maui.Controls.Effect"/>.</param>
		/// <remarks>Developers must supply a</remarks>
		public ExportEffectAttribute(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type effectType,
			string uniqueName)
		{
			if (uniqueName.IndexOf(".", StringComparison.Ordinal) != -1)
				throw new ArgumentException("uniqueName must not contain a .");
			Type = effectType;
			Id = uniqueName;
		}

		internal string Id { get; private set; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
		internal Type Type { get; private set; }
	}
}