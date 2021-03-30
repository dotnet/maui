using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public static class HotReloadExtensions
	{
		public static void CheckHandlers(this IView view)
		{
			if (view?.Handler == null)
				return;
			//So we can be smart and keep all old handlers
			//However with the Old Legacy Shim layouts, this causes issues.
			//So for now I am just going to kill all handlers, so everything needs rebuilt
			//var handlerType = handlerServiceProvider.GetHandlerType(view.GetType());
			//if (handlerType != view.Handler.GetType()){
			//	view.Handler = null;
			//}
			view.Handler = null;

			if (view is IContainer layout)
			{
				foreach (var v in layout.Children)
					CheckHandlers(v);
			}
		}

	}
}
