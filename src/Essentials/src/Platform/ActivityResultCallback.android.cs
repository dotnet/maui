using System;
using Android.Content;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.ApplicationModel
{
	class ActivityResultContractIntent : Object
	{
		public ActivityResultContractIntent(Func<Context,Intent> getIntent) => GetIntent = getIntent;

		public Func<Context,Intent> GetIntent { get; }
	}

	class ActivityResultContractResult : Object
	{
		public ActivityResultContractResult(int resultCode, Intent intent)
		{
			ResultCode = resultCode;
			Intent = intent;
		}

		internal int ResultCode { get; }
		internal Intent Intent { get; }
	}

	class CommonActivityResultContract : ActivityResultContract
	{
		internal Intent CreateIntent(Context context, ActivityResultContractIntent input)
			=> input!.GetIntent.Invoke(context);

		public override Intent CreateIntent(Context context, Object input)
			=> CreateIntent(context, input as ActivityResultContractIntent);

		public override Object ParseResult(int resultCode, Intent intent)
			=> new ActivityResultContractResult(resultCode, intent);
	}

	class CommonActivityResultCallback : Object, IActivityResultCallback
	{
		Action<ActivityResultContractResult> _action;

		public CommonActivityResultCallback(Action<ActivityResultContractResult> action)
		{
			_action = action;
		}

		public void OnActivityResult(Object p0)
			=> OnActivityResult(p0 as ActivityResultContractResult);

		void OnActivityResult(ActivityResultContractResult result)
			=> _action?.Invoke(result);
	}
}
