#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies the renderer type to use for a control.</summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RenderWithAttribute : Attribute
	{
		/// <summary>Creates a new <see cref="RenderWithAttribute"/> with the specified renderer type.</summary>
		public RenderWithAttribute([DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type type) : this(type, new[] { typeof(VisualMarker.DefaultVisual) })
		{
		}

		/// <summary>Creates a new <see cref="RenderWithAttribute"/> with the specified renderer type and supported visuals.</summary>
		public RenderWithAttribute(
			[DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type type,
			Type[] supportedVisuals)
		{
			Type = type;
			SupportedVisuals = supportedVisuals ?? new[] { typeof(VisualMarker.DefaultVisual) };
		}

		/// <summary>Gets the visual types supported by this renderer.</summary>
		public Type[] SupportedVisuals { get; }

		/// <summary>Gets the renderer type.</summary>
		[DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)]
		public Type Type { get; }
	}
}