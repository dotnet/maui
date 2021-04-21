using System;
using System.Collections.Generic;
using System.Text;
namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : IViewHandler
	{
		public static PropertyMapper<IWindow> LayoutMapper = new PropertyMapper<IWindow>(ViewHandler.ViewMapper)
		{
		};

		public WindowHandler() : base(LayoutMapper)
		{

		}

		public WindowHandler(PropertyMapper? mapper = null) : base(mapper ?? LayoutMapper)
		{

		}
	}
}
