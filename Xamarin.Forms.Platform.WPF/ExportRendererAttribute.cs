using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.WPF
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute
	{
		public ExportRendererAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportCellAttribute : HandlerAttribute
	{
		public ExportCellAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportImageSourceHandlerAttribute : HandlerAttribute
	{
		public ExportImageSourceHandlerAttribute(Type handler, Type target) : base(handler, target)
		{
		}
	}
}
