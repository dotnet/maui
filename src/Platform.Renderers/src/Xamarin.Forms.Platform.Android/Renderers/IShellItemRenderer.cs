using System;
using AndroidX.Fragment.App;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellItemRenderer : IDisposable
	{
		Fragment Fragment { get; }

		ShellItem ShellItem { get; set; }

		event EventHandler Destroyed;
	}
}