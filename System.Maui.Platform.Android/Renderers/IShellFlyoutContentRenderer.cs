using System;
using AView = Android.Views.View;

namespace System.Maui.Platform.Android
{
	public interface IShellFlyoutContentRenderer : IDisposable
	{
		AView AndroidView { get; }
	}
}