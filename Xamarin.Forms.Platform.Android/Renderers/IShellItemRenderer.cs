using Android.Support.V4.App;
using System;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellItemRenderer : IDisposable
	{
		Fragment Fragment { get; }

		ShellItem ShellItem { get; set; }

		event EventHandler Destroyed;
	}
}