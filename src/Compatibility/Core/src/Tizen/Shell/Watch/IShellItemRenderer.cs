using System;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Watch
{
	public interface IShellItemRenderer : IDisposable
	{
		BaseShellItem Item { get; }
		EvasObject NativeView { get; }
	}
}
