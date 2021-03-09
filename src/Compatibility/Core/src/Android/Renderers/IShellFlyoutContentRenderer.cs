using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellFlyoutContentRenderer : IDisposable
	{
		AView AndroidView { get; }
	}
}