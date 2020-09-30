using Android.Views.Accessibility;
using Xamarin.Forms.Platform.Android.FastRenderers;

namespace Xamarin.Forms.Platform.Android
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
				host.ContentDescription = $"{value}{AutomationPropertiesProvider.ConcatenateNameAndHelpText(_element)}";
			}
		}
	}
}