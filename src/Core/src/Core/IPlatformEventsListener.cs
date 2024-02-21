using System;
using System.Collections.Generic;
using System.Text;

#if ANDROID
using Android.Views;
#endif

namespace Microsoft.Maui
{
	/// <summary>
	/// This lets us wire into areas of a control that are only reachable via
	/// overriding but we don't want to override those controls.
	/// TODO: Expose this as a more permanent solution or create a better one
	/// </summary>
	internal interface IPlatformEventsListener

	{
#if ANDROID
		bool DispatchTouchEvent(MotionEvent? e);
#endif
	}
}
