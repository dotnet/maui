using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportHandlerAttribute : HandlerAttribute
	{
		public ExportHandlerAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}
}
