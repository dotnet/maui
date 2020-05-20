using System;
using AView = Android.Views.View;
using Android.Content;
using System.Maui.Core.Controls;

namespace System.Maui {
	public interface IAndroidViewRenderer : IViewRenderer
	{

		void SetContext(Context context);

		AView View { get; }
	}
}
