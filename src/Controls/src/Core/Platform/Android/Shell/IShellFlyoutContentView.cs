using System;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellFlyoutContentView : IDisposable
	{
		AView AndroidView { get; }
	}
}