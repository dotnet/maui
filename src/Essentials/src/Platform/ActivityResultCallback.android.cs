using System;
using AndroidX.Activity.Result;
using JavaObject = Java.Lang.Object;

namespace Microsoft.Maui.ApplicationModel;

class ActivityResultCallback<T>(Action<T> onActivityResult) : JavaObject, IActivityResultCallback
	where T : JavaObject
{
	readonly Action<T> _onActivityResult = onActivityResult;

	public void OnActivityResult(JavaObject result)
	{
		_onActivityResult.Invoke(result as T);
	}
}