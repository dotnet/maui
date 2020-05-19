using System;

namespace System.Maui
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute
	{
		public ExportRendererAttribute(Type handler, Type target) : this(handler, target, null)
		{
		}

		public ExportRendererAttribute(Type handler, Type target, Type[] supportedVisuals) : base(handler, target, supportedVisuals)
		{
		}
	}
}
