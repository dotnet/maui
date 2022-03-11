using Android.Content;
using AActivity = Android.App.Activity;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.AppLinks
{
	internal static class ContextExtensions
	{
		public static AActivity GetActivity(this Context context)
		{
			if (context == null)
				return null;

			if (context is AActivity activity)
				return activity;

			if (context is ContextWrapper contextWrapper)
				return contextWrapper.BaseContext.GetActivity();

			return null;
		}
	}
}
