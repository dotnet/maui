using System;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IDescendantFocusToggler
	{
		bool RequestFocus(global::Android.Views.View control, Func<bool> baseRequestFocus);
	}
}