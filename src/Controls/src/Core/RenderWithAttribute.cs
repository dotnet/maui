#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RenderWithAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.RenderWithAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RenderWithAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/RenderWithAttribute.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public RenderWithAttribute([DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type type) : this(type, new[] { typeof(VisualMarker.DefaultVisual) })
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RenderWithAttribute.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public RenderWithAttribute(
			[DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type type,
			Type[] supportedVisuals)
		{
			Type = type;
			SupportedVisuals = supportedVisuals ?? new[] { typeof(VisualMarker.DefaultVisual) };
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RenderWithAttribute.xml" path="//Member[@MemberName='SupportedVisuals']/Docs/*" />
		public Type[] SupportedVisuals { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/RenderWithAttribute.xml" path="//Member[@MemberName='Type']/Docs/*" />
		[DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)]
		public Type Type { get; }
	}
}