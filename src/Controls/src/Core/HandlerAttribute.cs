#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/HandlerAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.HandlerAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public abstract class HandlerAttribute : Attribute
	{
		protected HandlerAttribute(Type handler, [DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type target) : this(handler, target, null)
		{
		}

		protected HandlerAttribute(Type handler, [DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type target, Type[] supportedVisuals)
		{
			SupportedVisuals = supportedVisuals ?? new[] { typeof(VisualMarker.DefaultVisual) };
			TargetType = target;
			HandlerType = handler;
			Priority = 0;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/HandlerAttribute.xml" path="//Member[@MemberName='Priority']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public short Priority { get; set; }
		internal Type[] SupportedVisuals { get; private set; }
		internal Type HandlerType { get; private set; }

		[DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)]
		internal Type TargetType { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/HandlerAttribute.xml" path="//Member[@MemberName='ShouldRegister']/Docs/*" />
		public virtual bool ShouldRegister()
		{
			return true;
		}
	}
}
