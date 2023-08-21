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

using Android.Views.Accessibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	class EntryAccessibilityDelegate : global::Android.Views.View.AccessibilityDelegate
	{
		BindableObject _element;

		public EntryAccessibilityDelegate(BindableObject Element) : base()
		{
			_element = Element;
		}

		protected override void Dispose(bool disposing)
		{
			_element = null;
			base.Dispose(disposing);
		}

		public string ValueText { get; set; }

		public string ClassName { get; set; } = "android.widget.Button";

		public override void OnInitializeAccessibilityNodeInfo(global::Android.Views.View host, AccessibilityNodeInfo info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);
			info.ClassName = ClassName;
			if (_element != null)
			{
				var value = string.IsNullOrWhiteSpace(ValueText) ? string.Empty : $"{ValueText}. ";
				host.ContentDescription = $"{value}{Controls.Platform.AutomationPropertiesProvider.ConcatenateNameAndHelpText(_element)}";
			}
		}
	}
}