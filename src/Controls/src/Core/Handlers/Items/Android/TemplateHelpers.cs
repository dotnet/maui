using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class TemplateHelpers
	{

		public static INativeViewHandler GetHandler(View view, IMauiContext context)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}
			var handler = view.Handler;

			if (handler == null)
				handler = (INativeViewHandler)view.ToHandler(context);

			return (INativeViewHandler)handler;
		}
	}
}
