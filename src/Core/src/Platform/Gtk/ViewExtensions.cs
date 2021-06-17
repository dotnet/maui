using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Microsoft.Maui
{

	public static class ViewExtensions
	{

		public static void UpdateAutomationId(this Widget nativeView, IView view)
		{ }

		[PortHandler("implement drawing of other paints than solidpaint")]
		public static void UpdateBackground(this Widget nativeView, IView view)
		{
			var color = view.Background?.BackgroundColor;

			if (view.Background is SolidPaint solidPaint)
			{
				color = solidPaint.Color;
			}

			if (color == null)
				return;

			switch (nativeView)
			{
				case ProgressBar:
					nativeView.SetColor(color, "background-color", "trough > progress");

					break;
				case ComboBox box:
					// no effect: box.SetColor(bkColor, "border-color");
					box.GetCellRendererText().SetBackground(color);

					break;
				default:
					nativeView.SetBackgroundColor(color);

					break;
			}

		}
		
		public static void UpdateForeground(this Widget nativeView, Paint? paint)
		{
			var color = paint?.ForegroundColor;

			if (paint is SolidPaint solidPaint)
			{
				color = solidPaint.Color;
			}

			if (color == null)
				return;

			switch (nativeView)
			{
				case ProgressBar:
					nativeView.SetColor(color, "color", "trough > progress");

					break;
				case CheckButton:
					// no effect as check is an icon
					nativeView.SetColor(color, "color", "check");
					break;
				case ComboBox box:
					 box.GetCellRendererText().SetForeground(color);

					break;
				default:
					nativeView.SetForegroundColor(color);

					break;
			}

		}
		public static void UpdateIsEnabled(this Widget nativeView, IView view) =>
			nativeView?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateVisibility(this Widget nativeView, IView view) =>
			nativeView?.UpdateVisibility(view.Visibility);

		public static void UpdateSemantics(this Widget nativeView, IView view)
		{ }

		public static void UpdateOpacity(this Widget nativeView, IView view) { }

	}

}