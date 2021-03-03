using System;

namespace Microsoft.Maui.Controls.Compatibility
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportHandlerAttribute : HandlerAttribute
	{
		public ExportHandlerAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}
}
