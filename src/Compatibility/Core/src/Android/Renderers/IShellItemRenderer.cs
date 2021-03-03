using System;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellItemRenderer : IDisposable
	{
		Fragment Fragment { get; }

		ShellItem ShellItem { get; set; }

		event EventHandler Destroyed;
	}
}