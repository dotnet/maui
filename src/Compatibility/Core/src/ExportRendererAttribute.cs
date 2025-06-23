using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Compatibility
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute
	{
		public ExportRendererAttribute(Type handler, [DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type target) : this(handler, target, null)
		{
		}

		public ExportRendererAttribute(Type handler, [DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type target, Type[] supportedVisuals) : base(handler, target, supportedVisuals)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportCellAttribute : HandlerAttribute
	{
		public ExportCellAttribute(Type handler, [DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type target) : base(handler, target)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportImageSourceHandlerAttribute : HandlerAttribute
	{
		public ExportImageSourceHandlerAttribute(Type handler, [DynamicallyAccessedMembers(Internals.HandlerType.TargetMembers)] Type target) : base(handler, target)
		{
		}
	}
}