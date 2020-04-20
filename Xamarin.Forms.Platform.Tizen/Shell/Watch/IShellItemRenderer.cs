using System;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Watch
{
	public interface IShellItemRenderer : IDisposable
	{
		BaseShellItem Item { get; }
		EvasObject NativeView { get; }
	}
}
