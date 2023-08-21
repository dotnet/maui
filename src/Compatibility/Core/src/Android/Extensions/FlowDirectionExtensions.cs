//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Android.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class FlowDirectionExtensions
	{
		internal static void UpdateFlowDirection(this AView view, IVisualElementController controller)
		{
			if (view == null || controller == null)
				return;

			if (controller is IView v)
			{
				view.UpdateFlowDirection(v);
				return;
			}

			if (controller.EffectiveFlowDirection.IsRightToLeft())
			{
				view.LayoutDirection = ALayoutDirection.Rtl;

				if (view is TextView textView)
					textView.TextDirection = ATextDirection.Rtl;
			}
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
			{
				view.LayoutDirection = ALayoutDirection.Ltr;

				if (view is TextView textView)
					textView.TextDirection = ATextDirection.Ltr;
			}
		}
	}
}