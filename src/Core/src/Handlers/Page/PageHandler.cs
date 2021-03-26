using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : IViewHandler
	{
		public static PropertyMapper<IPage> LayoutMapper = new PropertyMapper<IPage>(ViewHandler.ViewMapper)
		{
		};

		public PageHandler() : base(LayoutMapper)
		{

		}

		public PageHandler(PropertyMapper? mapper = null) : base(mapper ?? LayoutMapper)
		{

		}
	}
}
