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

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal static class FlowDirectionExtensions
	{
		internal static bool UpdateFlowDirection(this UIView view, IVisualElementController controller)
		{
			if (controller == null || view == null)
				return false;

			if (controller is IView v)
			{
				var current = view.SemanticContentAttribute;
				view.UpdateFlowDirection(v);
				return current != view.SemanticContentAttribute;
			}

			UISemanticContentAttribute updateValue = view.SemanticContentAttribute;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				updateValue = UISemanticContentAttribute.ForceRightToLeft;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				updateValue = UISemanticContentAttribute.ForceLeftToRight;

			if (updateValue != view.SemanticContentAttribute)
			{
				view.SemanticContentAttribute = updateValue;
				return true;
			}

			return false;
		}

		internal static void UpdateTextAlignment(this UITextField control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
			{
				control.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
				control.TextAlignment = UITextAlignment.Right;
			}
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
			{
				control.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
				control.TextAlignment = UITextAlignment.Left;
			}
		}

		internal static void UpdateTextAlignment(this UITextView control, IVisualElementController controller)
		{
			if (controller == null || control == null)
				return;

			if (controller.EffectiveFlowDirection.IsRightToLeft())
				control.TextAlignment = UITextAlignment.Right;
			else if (controller.EffectiveFlowDirection.IsLeftToRight())
				control.TextAlignment = UITextAlignment.Left;
		}
	}
}