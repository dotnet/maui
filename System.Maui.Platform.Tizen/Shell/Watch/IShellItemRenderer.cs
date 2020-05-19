using System;
using ElmSharp;

namespace System.Maui.Platform.Tizen.Watch
{
	public interface IShellItemRenderer : IDisposable
	{
		BaseShellItem Item { get; }
		EvasObject NativeView { get; }
	}
}
