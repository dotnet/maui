using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{

	public static class ViewExtensions
	{

		public static void UpdateAutomationId(this Widget nativeView, IView view)
		{ }

		[PortHandler("implement drawing of other paints than solidpaint")]
		public static void UpdateBackground(this Widget nativeView, IView view)
		{
			if (view.Background is SolidPaint solidPaint)
			{
				nativeView.SetBackgroundColor(solidPaint.Color);
			}
			else if (view.Background is Paint paint)
			{
				nativeView.SetBackgroundColor(paint.BackgroundColor);
			}
			else
			{
				;
			}
		}

		public static void UpdateIsEnabled(this Widget nativeView, IView view) =>
			nativeView?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateVisibility(this Widget nativeView, IView view) =>
			nativeView?.UpdateVisibility(view.Visibility);

		public static void UpdateSemantics(this Widget nativeView, IView view)
		{
			
		}
		
		public static void UpdateOpacity(this Widget nativeView, IView view) { }

	}

}