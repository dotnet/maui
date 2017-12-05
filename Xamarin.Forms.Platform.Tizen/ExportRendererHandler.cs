using System;

namespace Xamarin.Forms.Platform.Tizen
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportHandlerAttribute : HandlerAttribute
	{
		public ExportHandlerAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}
}