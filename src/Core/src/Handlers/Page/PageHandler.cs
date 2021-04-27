#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : IViewHandler
	{
		public static PropertyMapper<IPage, PageHandler> PageMapper = new PropertyMapper<IPage, PageHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IPage.Title)] = MapTitle,
		};

		public PageHandler() : base(PageMapper)
		{

		}

		public PageHandler(PropertyMapper? mapper = null) : base(mapper ?? PageMapper)
		{

		}
	}
}
