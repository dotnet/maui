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

using Microsoft.UI.Xaml;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class FlowDirectionExtensions
	{
		internal static void UpdateFlowDirection(this FrameworkElement control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller is IView v)
			{
				control.UpdateFlowDirection(v);
				return;
			}

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				control.FlowDirection = WFlowDirection.RightToLeft;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				control.FlowDirection = WFlowDirection.LeftToRight;
		}
	}
}
