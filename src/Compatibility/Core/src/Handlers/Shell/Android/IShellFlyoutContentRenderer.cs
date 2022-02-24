using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellFlyoutContentRenderer : IDisposable
	{
		AView AndroidView { get; }
	}
}