#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.Core.View.Accessibility;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SemanticExtensions
	{
		public static void UpdateSemanticNodeInfo(this View virtualView, AccessibilityNodeInfoCompat? info)
		{
			foreach (var gesture in virtualView.GestureRecognizers)
			{
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					if (info != null)
						info.AddAction(AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);

				}
			}
		}
	}
}
