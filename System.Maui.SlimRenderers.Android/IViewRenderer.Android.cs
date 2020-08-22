using System;
using AView = Android.Views.View;
using Android.Content;

namespace System.Maui {
	public interface IAndroidViewRenderer : IMauiRenderer
	{
		void SetContext(Context context);

		AView View { get; }
	}
}
