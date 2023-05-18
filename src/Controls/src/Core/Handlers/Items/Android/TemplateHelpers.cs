#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class TemplateHelpers
	{

		public static IPlatformViewHandler GetHandler(View view, IMauiContext context)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}
			var handler = view.Handler;

			if (handler == null)
				handler = (IPlatformViewHandler)view.ToHandler(context);

			return (IPlatformViewHandler)handler;
		}
	}
}
