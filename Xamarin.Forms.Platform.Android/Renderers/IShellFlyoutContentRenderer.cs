using System;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellFlyoutContentRenderer : IDisposable
	{
		AView AndroidView { get; }
	}
}