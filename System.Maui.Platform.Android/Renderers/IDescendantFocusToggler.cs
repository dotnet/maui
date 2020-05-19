using System;

namespace System.Maui.Platform.Android
{
	internal interface IDescendantFocusToggler
	{
		bool RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus);
	}
}