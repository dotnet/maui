using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportCellAttribute : HandlerAttribute
	{
		public ExportCellAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}
}