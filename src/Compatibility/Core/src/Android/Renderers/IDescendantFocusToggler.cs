using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal interface IDescendantFocusToggler
	{
		bool RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus);
	}
}